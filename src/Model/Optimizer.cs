using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public abstract class Optimizer : IOptimizer
{
    public Constraints Constraints { get; }

    protected Simulation simulation;

    public Optimizer(World world, Constraints constraints)
    {
        Constraints = constraints;
        simulation = new Simulation(world);
    }

    /// <summary>
    /// Returns a new shift plan, with copied shifts.
    /// </summary>
    /// <param name="shiftPlan"></param>
    /// <returns></returns>
    public ShiftPlan GetShiftPlanWithShallowCopiedShiftsFrom(ShiftPlan shiftPlan)
    {
        List<Shift> shifts = new();
        foreach(Shift shift in shiftPlan.Shifts)
        {
            shifts.Add(new Shift(shift.Ambulance, shift.Depot, shift.Work));
        }

        return new ShiftPlan(shifts);
    }

    public abstract IEnumerable<ShiftPlan> FindOptimal(ShiftPlan shiftPlan, List<SuccessRatedIncidents> incidentsSets);
}

