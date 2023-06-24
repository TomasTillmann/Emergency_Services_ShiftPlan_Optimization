using ESSP.DataModel;

namespace Optimizing;

public abstract class MetaheuristicOptimizer : Optimizer
{
    protected MetaheuristicOptimizer(World world, Domain constraints) : base(world, constraints)
    {
    }
}
