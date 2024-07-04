using ESSP.DataModel;
using Optimizing;

namespace Optimizng;

public abstract class NeighbourOptimizer : OptimizerBase
{
  public IMoveGenerator MoveGenerator { get; set; }

  public NeighbourOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator)
  : base(world, constraints, utilityFunction)
  {
    MoveGenerator = moveGenerator;
  }

  protected void ModifyMakeMove(EmergencyServicePlan plan, MoveSequence moveSequence)
  {
    for (int i = 0; i < moveSequence.Count; ++i)
    {
      DoMove(plan, moveSequence.MovesBuffer[i]);
    }
  }

  protected void ModifyMakeInverseMove(EmergencyServicePlan plan, MoveSequence moveSequence)
  {
    for (int i = moveSequence.Count - 1; i >=0; --i)
    {
      DoMove(plan, moveSequence.MovesBuffer[i]);
    }
  }

  private void DoMove(EmergencyServicePlan plan, Move move)
  {
    switch (move.Type)
    {
      case MoveType.Identity:
        break;

      case MoveType.TeamAllocation:
        plan.AllocateTeam(move.DepotIndex, move.Shift);
        break;

      case MoveType.TeamDeallocation:
        plan.DeallocateTeam(move.DepotIndex, move.OnDepotIndex);
        break;

      case MoveType.ShiftChange:
        plan.ChangeShift(move.DepotIndex, move.OnDepotIndex, move.Shift);
        break;

      case MoveType.AmbulanceAllocation:
        plan.AllocateAmbulance(move.DepotIndex);
        break;

      case MoveType.AmbulanceDeallocation:
        plan.DeallocateAmbulance(move.DepotIndex);
        break;
    }
  }
}
