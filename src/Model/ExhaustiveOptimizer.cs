using ESSP.DataModel;
using Logging;
using Model.Extensions;
using Simulating;

namespace Optimizing;

public sealed class ExhaustiveOptimizer : Optimizer
{
    public ExhaustiveOptimizer(World world, Constraints constraints) : base(world, constraints)
    {
        if(constraints.AllowedShiftStartingTimes.Count() == 0 || constraints.AllowedShiftDurations.Count() == 0)
        {
            throw new ArgumentException("Constraints need to be set");
        }
    } 

    /// <summary>
    /// Tries brute force search for all possible combinations of starting times and shift durations on all shifts.
    /// Keeps all shiftPlans in memory, to then globally assess which shift plan has the minimum cost and at the same time satisfies minimum threshold on all given historical incidents.
    /// </summary>
    /// <param name="shiftPlan">Shift plan structure, on which all possible shifts combinations are traversed.</param>
    /// <param name="incidentsSets">Historical incidents.</param>
    /// <returns></returns>
    public override IEnumerable<ShiftPlan> FindOptimal(ShiftPlan shiftPlan, List<IncidentsSet> incidentsSets)
    {
        Logger.Instance.WriteLineForce(incidentsSets[0].Value.Visualize(separator: "\n"));

        shiftPlan.Shifts.ForEach(shift => shift.Work = Interval.GetByStartAndDuration(Constraints.AllowedShiftStartingTimes.First(), Constraints.AllowedShiftDurations.First()));

        List<ShiftPlan> allShiftPlans = new();

        HashSet<Seconds> allowedDurations = new(Constraints.AllowedShiftDurations)
        {
            0.ToSeconds()
        };


        bool Succeeds(ShiftPlan shiftPlan)
        {
            foreach(IncidentsSet incidentsSet in incidentsSets)
            {
                Statistics stats = simulation.Run(incidentsSet.Value, shiftPlan);

                if(stats.SuccessRate < incidentsSet.Threshold)
                {
                    return false;
                }
            }

            return true;
        }

        void PopulateAllSuccessfulShiftPlansFrom(ShiftPlan shiftPlan, int currentShiftIndex = 0)
        {
            if(currentShiftIndex == shiftPlan.Shifts.Count)
            {
                if (Succeeds(shiftPlan))
                {
                    allShiftPlans.Add(GetShiftPlanWithShallowCopiedShiftsFrom(shiftPlan));
                }

                return;
            }

            foreach (Seconds duration in allowedDurations)
            {
                if(duration == 0.ToSeconds())
                {
                    Interval originalWork = shiftPlan.Shifts[currentShiftIndex].Work;
                    shiftPlan.Shifts[currentShiftIndex].Work = Interval.GetByStartAndDuration(0.ToSeconds(), 0.ToSeconds());

                    PopulateAllSuccessfulShiftPlansFrom(shiftPlan, currentShiftIndex + 1);

                    shiftPlan.Shifts[currentShiftIndex].Work = originalWork; 

                    break;
                }

                foreach (Seconds startingTime in Constraints.AllowedShiftStartingTimes)
                {
                    //Logger.Instance.WriteLineForce($"Shift: {shift}, startingTime: {startingTime}, duration: {duration}");

                    Interval originalWork = shiftPlan.Shifts[currentShiftIndex].Work;
                    shiftPlan.Shifts[currentShiftIndex].Work = Interval.GetByStartAndDuration(startingTime, duration);

                    PopulateAllSuccessfulShiftPlansFrom(shiftPlan, currentShiftIndex + 1);

                    shiftPlan.Shifts[currentShiftIndex].Work = originalWork; 
                }
            }

            Logger.Instance.WriteLineForce($"Finished for: {currentShiftIndex}");
        }

        // traverse the state space
        PopulateAllSuccessfulShiftPlansFrom(shiftPlan);

        Logger.Instance.WriteLineForce();
        Logger.Instance.WriteLineForce(allShiftPlans.Visualize(separator: "\n"));

        if(allShiftPlans.Count == 0)
        {
            return Enumerable.Empty<ShiftPlan>(); 
        }

        return allShiftPlans.FindMinSubset(shiftPlan => shiftPlan.GetCost());
    }

}

