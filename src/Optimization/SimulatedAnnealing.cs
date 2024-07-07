using ESSP.DataModel;

namespace Optimizing;

public class SimulatedAnnealing : NeighbourOptimizer
{
  public SimulatedAnnealing(World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
  }

  public override List<EmergencyServicePlan> GetBest(ReadOnlySpan<Incident> incidents)
  {
    throw new NotImplementedException();
  }
}


