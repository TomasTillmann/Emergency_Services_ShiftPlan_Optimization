using ESSP.DataModel;
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
        ShiftPlanTabu maxShiftPlan = ShiftPlanTabu.GetInitial(world.Depots, SimulationDuration);

        if(!IsValid(maxShiftPlan.Value, incidentsSets))
        {
            return Enumerable.Empty<ShiftPlan>();
        }

        ShiftPlanTabu globalBest = maxShiftPlan;
        ShiftPlanTabu bestCandidate = maxShiftPlan;
        LinkedList<ShiftPlanTabu> tabu = new();
        List<ShiftPlanTabu> neighbourHood = new(); 
        tabu.AddLast(maxShiftPlan);

        for(int i = 0; i < Iterations; i++)
        {
            neighbourHood = GetNeighborhood(bestCandidate);

            bestCandidate = neighbourHood.First();
            foreach (ShiftPlanTabu candidate in neighbourHood)
            {
                if (!tabu.Contains(candidate) && Fitness(candidate, incidentsSets) < Fitness(bestCandidate, incidentsSets))
                {
                    bestCandidate = candidate;
                }
            }

            if (Fitness(bestCandidate, incidentsSets) < Fitness(globalBest, incidentsSets))
            {
                globalBest = bestCandidate;
            }

            tabu.AddLast(bestCandidate);
            if (tabu.Count > MaxTabuSize)
            {
                tabu.RemoveFirst();
            }
        }

        return new List<ShiftPlan> { globalBest.Value };
    }

    private int Fitness(ShiftPlanTabu shiftPlan, List<SuccessRatedIncidents> successRatedIncidents)
    {
        int eval = base.Fitness(shiftPlan.Value, successRatedIncidents, out double meanSuccessRate);
        if(eval == int.MaxValue)
        {
            return (int)(shiftPlan.Value.GetCost() * (1 - meanSuccessRate));
        }

        return eval + this.InitialDurationPenalty * shiftPlan.ShiftsOfInitialDurationCount; 
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
