using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Move generated by implementation of <see cref="MoveGeneratorBase"/> is made by this class.
/// </summary>
public class MoveMaker
{
  /// <summary>
  /// Makes the normal move on given <see paramref="plan/>.
  /// </summary>
  public void ModifyMakeMove(EmergencyServicePlan plan, MoveSequence moveSequence)
  {
    for (int i = 0; i < moveSequence.Count; ++i)
    {
      DoMove(plan, moveSequence.MovesBuffer[i]);
    }
  }

  /// <summary>
  /// Makes the inverse move on given <see paramref="plan/>.
  /// The inverse move is iterated from back to front.
  /// </summary>
  public void ModifyMakeInverseMove(EmergencyServicePlan plan, MoveSequence moveSequence)
  {
    for (int i = moveSequence.Count - 1; i >= 0; --i)
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

