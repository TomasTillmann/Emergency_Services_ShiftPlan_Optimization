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

    public abstract IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets);

    protected ShiftPlan GetEmptyShiftPlan()
    {
        return ShiftPlan.ConstructFrom(simulation.Depots, 0.ToSeconds(), 0.ToSeconds());
    }
}

