using ESSP.DataModel;
using Logging;
using Model.Extensions;
using Simulating;

namespace Optimizing;

/// <summary>
/// Tries all possible combinations of starting times and shift durations on all shifts.
/// </summary>
public sealed class ExhaustiveOptimizer : Optimizer
{
    public ExhaustiveOptimizer(World world, Constraints constraints) : base(world, constraints) { }

    public override ShiftPlan FindOptimal(ShiftPlan shiftPlan, List<IncidentsSet> incidentsSets)
    {
        // traverse the state space
        return Traverse(incidentsSets, shiftPlan, shiftPlan, 0);
    }

    private ShiftPlan Traverse(IReadOnlyList<IncidentsSet> incidentsSets, ShiftPlan optimalShiftPlan, ShiftPlan currentShiftPlan, int shift)
    {
        if(shift == currentShiftPlan.Shifts.Count)
        {
            foreach(IncidentsSet incidentsSet in incidentsSets)
            {
                Statistics stats = simulation.Run(incidentsSet.Value, currentShiftPlan);
                if(stats.SuccessRate < incidentsSet.Threshold)
                {
                    return optimalShiftPlan;
                }
            }

            return currentShiftPlan.GetCost() < optimalShiftPlan.GetCost() ? currentShiftPlan.CopyTillShifts() : optimalShiftPlan;
        }

        foreach(Seconds startingTime in Constraints.AllowedShiftStartingTimes)
        {
            foreach(Seconds duration in Constraints.AllowedShiftDurations)
            {
                //Logger.Instance.WriteLineForce($"Shift: {shift}, startingTime: {startingTime}, duration: {duration}");
                Interval originalWork = currentShiftPlan.Shifts[shift].Work;
                currentShiftPlan.Shifts[shift].Work = Interval.GetByStartAndDuration(startingTime, duration);

                optimalShiftPlan = Traverse(incidentsSets, optimalShiftPlan, currentShiftPlan, shift + 1);

                currentShiftPlan.Shifts[shift].Work = originalWork; 
            }
        }

        Logger.Instance.WriteLineForce($"Finished for: {shift}");

        return optimalShiftPlan; 
    }
}

