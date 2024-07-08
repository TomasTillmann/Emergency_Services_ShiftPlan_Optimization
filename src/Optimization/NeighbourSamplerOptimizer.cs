using ESSP.DataModel;

namespace Optimizing;

public abstract class NeighbourSamplerOptimizer : OptimizerBase
{
  public IRandomMoveSampler RandomMoveSampler { get; set; }
  private readonly MoveMaker _moveMaker;

  public NeighbourSamplerOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction, IRandomMoveSampler randomMoveSampler)
  : base(world, constraints, utilityFunction)
  {
    RandomMoveSampler = randomMoveSampler;
    _moveMaker = new();
  }

  protected void ModifyMakeMove(EmergencyServicePlan plan, MoveSequence moveSequence)
  {
    _moveMaker.ModifyMakeMove(plan, moveSequence);
  }

  protected void ModifyMakeInverseMove(EmergencyServicePlan plan, MoveSequence moveSequence)
  {
    _moveMaker.ModifyMakeInverseMove(plan, moveSequence);
  }
}

