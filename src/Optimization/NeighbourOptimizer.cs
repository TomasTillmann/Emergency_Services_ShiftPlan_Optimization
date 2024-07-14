using ESSP.DataModel;
using Optimizing;

namespace Optimizing;

/// <summary>
/// Adds functionality for move making using <see cref="IMoveGenerator" />.
/// </summary>
public abstract class NeighbourOptimizer : OptimizerBase
{
  public IMoveGenerator MoveGenerator { get; set; }
  private readonly MoveMaker _moveMaker;

  public NeighbourOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator)
  : base(world, constraints, utilityFunction)
  {
    MoveGenerator = moveGenerator;
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
