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
        public ShiftPlan Value;

        private ShiftPlanTabu(ShiftPlan shiftPlan)
        {
            Value = shiftPlan;
        }

        public static ShiftPlanTabu GetAllShiftHavingSameDuration(IReadOnlyList<Depot> depots, Seconds duration)
        {
            return new ShiftPlanTabu(ShiftPlan.ConstructFrom(depots, 0.ToSeconds(), duration));
        }
        public static ShiftPlanTabu GetRandom(IReadOnlyList<Depot> depots, List<Seconds> allowedStartingTimes, List<Seconds> allowedShiftDurations, Random? random = null)
        {
            ShiftPlan shiftPlanDefault = ShiftPlan.ConstructEmpty(depots);
            foreach(Shift shift in shiftPlanDefault.Shifts)
            {
                shift.Work = Interval.GetByStartAndDuration(allowedStartingTimes.GetRandom(random), allowedShiftDurations.GetRandom(random));
            }

            return new ShiftPlanTabu(shiftPlanDefault); 
        }

        internal ShiftPlanTabu Clone()
        {
            return new ShiftPlanTabu(Value.Clone());
        }

        public override string ToString()
        {
            return Value.Shifts.Select(shift => (shift.Work.Start.Value / 60/ 60, shift.Work.End.Value / 60/ 60)).Visualize(",");
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
    #endregion

    private Seconds MaxDuration;
    private Seconds EarliestStartingTime;

    private List<Seconds> AllowedDurationsSorted;
    private List<Seconds> AllowedStartingTimesSorted;

    private readonly int maxShiftPlanCost;

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
    public TabuSearchOptimizer(World world, Constraints constraints, int iterations, int maxTabuSize) : base(world, constraints)
    {
        this.Iterations = iterations;
        this.MaxTabuSize = maxTabuSize;

        AllowedDurationsSorted = Constraints.AllowedShiftDurations.OrderBy(d => d.Value).ToList();
        MaxDuration = AllowedDurationsSorted.Last();

        ShiftPlan maximalShiftPlan = ShiftPlanTabu.GetAllShiftHavingSameDuration(world.Depots, MaxDuration).Value;
        this.maxShiftPlanCost = maximalShiftPlan.GetCost();

        AllowedStartingTimesSorted = Constraints.AllowedShiftStartingTimes.OrderBy(startingTime => startingTime.Value).ToList();
        EarliestStartingTime = Constraints.AllowedShiftStartingTimes.First();
    }

    public override IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets)
    {
        int Fitness(ShiftPlanTabu shiftPlanTabu)
        {
            return this.Fitness(shiftPlanTabu.Value, incidentsSets);
        }

        ShiftPlanTabu initShiftPlan
            = ShiftPlanTabu.GetRandom(world.Depots, Constraints.AllowedShiftStartingTimes.ToList(), Constraints.AllowedShiftDurations.ToList());

        ShiftPlanTabu globalBest = initShiftPlan;
        int globalBestFitness = Fitness(globalBest);

        ShiftPlanTabu? bestCandidate = initShiftPlan;
        int? bestCandidateFitness = Fitness(bestCandidate);

        LinkedList<ShiftPlanTabu> tabu = new();
        tabu.AddLast(initShiftPlan);

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

            Logger.Instance.WriteLineForce($"Global best: {globalBestFitness} ({globalBest.Value})");
            Logger.Instance.WriteLineForce($"Best candidate: {bestCandidateFitness} ({bestCandidate.Value})");
            Logger.Instance.WriteLineForce();
        }

        return new List<ShiftPlan> { globalBest.Value };
    }

    internal int Fitness(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents)
    {
        int eval = base.Fitness(shiftPlan, successRatedIncidents, out double meanSuccessRate);

        // damping, to better navigate
        if(eval == int.MaxValue)
        {
            //int cost = shiftPlan.GetCost();
            return maxShiftPlanCost + (int)((1 - meanSuccessRate) * 100);
            //return int.MaxValue;
        }

        return eval;
    } 

    private List<ShiftPlanTabu> GetNeighborhood(ShiftPlanTabu shiftPlanTabu)
    {
        List<ShiftPlanTabu> neighborhood = new();
        Shift shift;
        ShiftPlanTabu neighbour;

        for (int i = 0; i < shiftPlanTabu.Value.Shifts.Count; i++)
        {
            // Making shorter
            neighbour = shiftPlanTabu.Clone();
            shift = neighbour.Value.Shifts[i];
            if (shift.Work != Interval.Empty)
            {
                Seconds? duration = GetClosestAllowedDurationInDirection(shift.Work.Duration, left: true);
                if(duration is null)
                {
                    shift.Work = Interval.Empty;
                }
                else
                {
                    shift.Work = Interval.GetByStartAndDuration(shift.Work.Start, duration.Value);
                }

                neighborhood.Add(neighbour);
            }

            // Making longer
            neighbour = shiftPlanTabu.Clone();
            shift = neighbour.Value.Shifts[i];
            if (shift.Work.Duration != MaxDuration)
            {
                Seconds duration = GetClosestAllowedDurationInDirection(shift.Work.Duration, left: false)!.Value;
                shift.Work = Interval.GetByStartAndDuration(shift.Work.Start, duration);

                neighborhood.Add(neighbour);
            }

            // Moving right 
            neighbour = shiftPlanTabu.Clone();
            shift = neighbour.Value.Shifts[i];
            if (shift.Work != Interval.Empty)
            {
                Seconds? startingTime = GetClosestAllowedStartingTimeInDirection(shift.Work.Start, left: false);
                if (startingTime is not null)
                {
                    shift.Work = Interval.GetByStartAndDuration(startingTime.Value, shift.Work.Duration);
                }

                neighborhood.Add(neighbour);
            }

            // Moving left 
            neighbour = shiftPlanTabu.Clone();
            shift = neighbour.Value.Shifts[i];
            if (shift.Work != Interval.Empty)
            {
                Seconds? startingTime = GetClosestAllowedStartingTimeInDirection(shift.Work.Start, left: true);
                if(startingTime is not null)
                {
                    shift.Work = Interval.GetByStartAndDuration(startingTime.Value, shift.Work.Duration);
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
