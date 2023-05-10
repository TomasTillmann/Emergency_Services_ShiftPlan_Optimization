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

    public abstract ShiftPlan FindOptimal(ShiftPlan shiftPlan, List<IncidentsSet> incidentsSets);
}

