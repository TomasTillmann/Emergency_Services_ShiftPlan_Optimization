using ESSP.DataModel;
using Logging;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimizing;

public sealed class TabuSearchOptimizer : Optimizer
{
    private class ShiftPlanTabu
    {
        public int ShiftsOfInitialDurationCount { get; private set; } = 0;
        public ShiftPlan Value;

        private Seconds initialDuration;

        private ShiftPlanTabu(ShiftPlan shiftPlan, Seconds initialDuration)
        {
            Value = shiftPlan;
            this.initialDuration = initialDuration;
            ShiftsOfInitialDurationCount = Value.Shifts.Count;
        }

        public static ShiftPlanTabu GetInitial(IReadOnlyList<Depot> depots, Seconds initialDuration)
        {
            return new ShiftPlanTabu(ShiftPlan.ConstructFrom(depots, 0.ToSeconds(), initialDuration), initialDuration);
        }

        public void SetShiftsWork(int index, Interval work)
        {
            if(work.Duration != initialDuration)
            {
                ShiftsOfInitialDurationCount--;
            }

            if(work.Duration == initialDuration)
            {
                ShiftsOfInitialDurationCount++;
            }

            Value.Shifts[index].Work = work;
        }

        internal ShiftPlanTabu Clone()
        {
            return new ShiftPlanTabu(Value.Clone(), initialDuration);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(ShiftPlanTabu s1, ShiftPlanTabu s2)
        {
            List<Shift>.Enumerator thisEnumerator = s1.Value.Shifts.GetEnumerator();
            List<Shift>.Enumerator anotherEnumerator = s2.Value.Shifts.GetEnumerator();

            while (thisEnumerator.MoveNext() && anotherEnumerator.MoveNext())
            {
                if(thisEnumerator.Current.Work != anotherEnumerator.Current.Work)
                {
                    return false;
                }
            }

            // I know they will be same size - so I don't test that.
            return true;
        }

        public static bool operator !=(ShiftPlanTabu s1, ShiftPlanTabu s2) => !(s1 == s2);

        public override bool Equals(object? obj)
        {
            if(obj is ShiftPlanTabu s)
            {
                return this == s;
            }

            return false;
        }
    }

    #region Params
    public readonly int Iterations;
    public readonly int MaxTabuSize;
    public readonly int InitialDurationPenalty;
    public readonly Seconds SimulationDuration;
    #endregion

    private Seconds MaxDuration;
    private Seconds EarliestStartingTime;

    private List<Seconds> AllowedDurationsSorted;
    private List<Seconds> AllowedStartingTimesSorted;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="world"></param>
    /// <param name="constraints"></param>
    /// <param name="iterations"></param>
    /// <param name="maxTabuSize">Shoudl be less or equal to minimal number of possible neighbours (or some mean at least, to minimize the change),
    /// otherwise could happen, that all neighbours are tabu and for all aspiration criterion is not satisfied, leading to no allowed moves.</param>
    /// <param name="simulationDuration"></param>
    /// <param name="initialDurationPenalty"></param>
    public TabuSearchOptimizer(World world, Constraints constraints, int iterations, int maxTabuSize, Seconds simulationDuration, int initialDurationPenalty) : base(world, constraints)
    {
        this.Iterations = iterations;
        this.MaxTabuSize = maxTabuSize;
        this.SimulationDuration = simulationDuration;
        this.InitialDurationPenalty = initialDurationPenalty;

        MaxDuration = simulationDuration; 
        EarliestStartingTime = Constraints.AllowedShiftStartingTimes.Min();

        AllowedDurationsSorted = new List<Seconds>(Constraints.AllowedShiftDurations)
        {
            simulationDuration
        };
        AllowedDurationsSorted.Sort((d1,d2) => d1.Value.CompareTo(d2.Value));

        AllowedStartingTimesSorted = Constraints.AllowedShiftStartingTimes.OrderBy(startingTime => startingTime.Value).ToList();
    }

    public override IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets)
    {
        int Fitness(ShiftPlanTabu shiftPlanTabu)
        {
            return this.Fitness(shiftPlanTabu, incidentsSets);
        }

        ShiftPlanTabu maxShiftPlan = ShiftPlanTabu.GetInitial(world.Depots, SimulationDuration);

        if(!IsValid(maxShiftPlan.Value, incidentsSets))
        {
            return Enumerable.Empty<ShiftPlan>();
        }

        ShiftPlanTabu globalBest = maxShiftPlan;
        int globalBestFitness = Fitness(globalBest);

        ShiftPlanTabu? bestCandidate = maxShiftPlan;
        int? bestCandidateFitness = Fitness(bestCandidate);

        LinkedList<ShiftPlanTabu> tabu = new();
        tabu.AddLast(maxShiftPlan);

        for(int i = 0; i < Iterations; i++)
        {
            List<ShiftPlanTabu> neighbourHood = GetNeighborhood(bestCandidate);

            bestCandidate = null;
            bestCandidateFitness = null;
            foreach (ShiftPlanTabu candidate in neighbourHood)
            {
                int candidateFitness = Fitness(candidate);

                if (tabu.Contains(candidate))
                {
                    // aspiration criterion
                    if(candidateFitness < globalBestFitness)
                    {
                        if(bestCandidate is null || candidateFitness < bestCandidateFitness)
                        {
                            bestCandidate = candidate;
                            bestCandidateFitness = candidateFitness;
                        }
                    }
                }
                else
                {
                    if(bestCandidate is null || candidateFitness < bestCandidateFitness)
                    {
                        bestCandidate = candidate;
                        bestCandidateFitness = candidateFitness;
                    }
                }
            }

            if(bestCandidate is null || bestCandidateFitness is null)
            {
                throw new ArgumentException("All neighbours were tabu and none of them also satisfied aspiration criterion. Perhaps you set tabu size too high?");
            }

            Logger.Instance.WriteLineForce($"Best candidate: {bestCandidate}");
            Logger.Instance.WriteLineForce($"Tabu: {tabu.Visualize("|")}");

            if (bestCandidateFitness < globalBestFitness)
            {
                globalBest = bestCandidate;
                globalBestFitness = bestCandidateFitness.Value;
            }

            tabu.AddLast(bestCandidate);
            if (tabu.Count > MaxTabuSize)
            {
                tabu.RemoveFirst();
            }

            Logger.Instance.WriteLineForce($"Global best: {Fitness(globalBest)} | {globalBest.Value}");
            Logger.Instance.WriteLineForce($"Best candidate: {Fitness(bestCandidate)} | {bestCandidate.Value}");
            Logger.Instance.WriteLineForce();
        }

        return new List<ShiftPlan> { globalBest.Value };
    }

    private int Fitness(ShiftPlanTabu shiftPlan, List<SuccessRatedIncidents> successRatedIncidents)
    {
        int eval = base.Fitness(shiftPlan.Value, successRatedIncidents, out double meanSuccessRate);
        if(eval == int.MaxValue)
        {
            //return (int)(shiftPlan.Value.GetCost() * (1 - meanSuccessRate));
            return int.MaxValue;
        }

        //return eval + this.InitialDurationPenalty * shiftPlan.ShiftsOfInitialDurationCount; 
        return eval + this.InitialDurationPenalty;
    } 

    private List<ShiftPlanTabu> GetNeighborhood(ShiftPlanTabu shiftPlanTabu)
    {
        List<ShiftPlanTabu> neighborhood = new();
        Shift readonlyShift;
        ShiftPlanTabu neighbour;

        for (int i = 0; i < shiftPlanTabu.Value.Shifts.Count; i++)
        {
            // Making shorter
            neighbour = shiftPlanTabu.Clone();
            readonlyShift = neighbour.Value.Shifts[i];
            if (readonlyShift.Work != Interval.Empty)
            {
                Seconds? duration = GetClosestAllowedDurationInDirection(readonlyShift.Work.Duration, left: true);
                if(duration is null)
                {
                    neighbour.SetShiftsWork(i, Interval.Empty);
                }
                else
                {
                    neighbour.SetShiftsWork(i, Interval.GetByStartAndDuration(readonlyShift.Work.Start, duration.Value));
                }

                neighborhood.Add(neighbour);
            }

            // Making longer
            neighbour = shiftPlanTabu.Clone();
            readonlyShift = neighbour.Value.Shifts[i];
            if (readonlyShift.Work.Duration != MaxDuration)
            {
                Seconds duration = GetClosestAllowedDurationInDirection(readonlyShift.Work.Duration, left: false)!.Value;
                neighbour.SetShiftsWork(i, Interval.GetByStartAndDuration(readonlyShift.Work.Start, duration));

                neighborhood.Add(neighbour);
            }

            // Moving right 
            neighbour = shiftPlanTabu.Clone();
            readonlyShift = neighbour.Value.Shifts[i];
            if (readonlyShift.Work != Interval.Empty)
            {
                Seconds? startingTime = GetClosestAllowedStartingTimeInDirection(readonlyShift.Work.Start, left: false);
                if (startingTime is not null)
                {
                    neighbour.SetShiftsWork(i, Interval.GetByStartAndDuration(startingTime.Value, readonlyShift.Work.Duration));
                }

                neighborhood.Add(neighbour);
            }


            // Moving left 
            neighbour = shiftPlanTabu.Clone();
            readonlyShift = neighbour.Value.Shifts[i];
            if (readonlyShift.Work != Interval.Empty)
            {
                Seconds? startingTime = GetClosestAllowedStartingTimeInDirection(readonlyShift.Work.Start, left: true);
                if(startingTime is not null)
                {
                    neighbour.SetShiftsWork(i, Interval.GetByStartAndDuration(startingTime.Value, readonlyShift.Work.Duration));
                    neighborhood.Add(neighbour);
                }
            }
        }

        return neighborhood;
    }

    private Seconds? GetClosestAllowedDurationInDirection(Seconds duration, bool left)
    {
        return GetClosestFromListInDirection(duration, AllowedDurationsSorted, left);
    }

    private Seconds? GetClosestAllowedStartingTimeInDirection(Seconds duration, bool left)
    {
        return GetClosestFromListInDirection(duration, AllowedStartingTimesSorted, left);
    }

    private Seconds? GetClosestFromListInDirection(Seconds duration, List<Seconds> list, bool left)
    {
        int index = list.IndexOf(duration);

        if (left)
        {
            if(index == 0)
            {
                return null;
            }

            return list[index - 1];
        }
        else
        {
            if(index == list.Count - 1)
            {
                return null;
            }

            return list[index + 1];
        }
    }
}
