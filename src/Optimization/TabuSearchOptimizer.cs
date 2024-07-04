using ESSP.DataModel;

namespace Optimizing;

public class TabuSearchOptimizer : OptimizerBase
{
  public TabuSearchOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction)
  : base(world, constraints, utilityFunction)
  {
  }

  public override List<EmergencyServicePlan> GetBest(ReadOnlySpan<Incident> incidents)
  {
    throw new NotImplementedException();
  }
}

