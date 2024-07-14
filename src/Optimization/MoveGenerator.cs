using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Provides moves like ambulance allocations, team allocation, shift change ... which are very useful for every specific move generator.
/// </summary>
public abstract class MoveGeneratorBase : IMoveGenerator
{
  /// <summary>
  /// The size of the moves buffer.
  /// Maximum length of allowed moves sequence.
  /// </summary>
  public int MovesBufferSize { get; }
  
  /// <summary>
  /// Allowed shift times.
  /// </summary>
  protected ShiftTimes ShiftTimes { get; set; }
  
  /// <summary>
  /// Constraints.
  /// </summary>
  protected Constraints Constraints { get; set; }
  
  /// <summary>
  /// The shared moves instance. Shared, to minimize heap allocations.
  /// </summary>
  protected MoveSequenceDuo Moves { get; set; }

  public MoveGeneratorBase(ShiftTimes shiftTimes, Constraints constraints, int movesBufferSize)
  {
    ShiftTimes = shiftTimes;
    Constraints = constraints;
    MovesBufferSize = movesBufferSize;
    Moves = new MoveSequenceDuo
    {
      Normal = MoveSequence.GetNewEmpty(movesBufferSize),
      Inverse = MoveSequence.GetNewEmpty(movesBufferSize)
    };
  }

  /// <inheritdoc />
  public abstract IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan);

  protected void Identity()
  {
    Moves.Count = 1;
    Moves.Normal.MovesBuffer[0] = new Move
    {
      Type = MoveType.Identity
    };
    Moves.Inverse.MovesBuffer[0] = new Move
    {
      Type = MoveType.Identity
    };
  }

  protected void ChangeShift(MedicTeamId teamId, Interval oldShift, Interval newShift)
  {
    Moves.Count = 1;
    Moves.Inverse.MovesBuffer[0] = new Move
    {
      Type = MoveType.ShiftChange,
      Shift = oldShift,
      DepotIndex = teamId.DepotIndex,
      OnDepotIndex = teamId.OnDepotIndex
    };

    Moves.Normal.MovesBuffer[0] = new Move
    {
      Type = MoveType.ShiftChange,
      Shift = newShift,
      DepotIndex = teamId.DepotIndex,
      OnDepotIndex = teamId.OnDepotIndex
    };
  }

  protected void AllocateTeamAndAmbulance(int depotIndex, Interval newShift, int lastIndex)
  {
    Moves.Count = 2;
    Moves.Inverse.MovesBuffer[0] = new Move
    {
      Type = MoveType.TeamDeallocation,
      DepotIndex = depotIndex,
      OnDepotIndex = lastIndex
    };

    Moves.Inverse.MovesBuffer[1] = new Move
    {
      Type = MoveType.AmbulanceDeallocation,
      DepotIndex = depotIndex,
    };

    Moves.Normal.MovesBuffer[1] = new Move
    {
      Type = MoveType.AmbulanceAllocation,
      DepotIndex = depotIndex,
    };

    Moves.Normal.MovesBuffer[0] = new Move
    {
      Type = MoveType.TeamAllocation,
      DepotIndex = depotIndex,
      Shift = newShift
    };
  }

  protected void AllocateTeam(int depotIndex, Interval newShift, int lastIndex)
  {
    Moves.Count = 1;
    Moves.Inverse.MovesBuffer[0] = new Move
    {
      Type = MoveType.TeamDeallocation,
      DepotIndex = depotIndex,
      OnDepotIndex = lastIndex
    };

    Moves.Normal.MovesBuffer[0] = new Move
    {
      Type = MoveType.TeamAllocation,
      DepotIndex = depotIndex,
      Shift = newShift
    };
  }

  protected void AllocateAmbulance(int depotIndex)
  {
    Moves.Count = 1;
    Moves.Inverse.MovesBuffer[0] = new Move
    {
      Type = MoveType.AmbulanceDeallocation,
      DepotIndex = depotIndex,
    };

    Moves.Normal.MovesBuffer[0] = new Move
    {
      Type = MoveType.AmbulanceAllocation,
      DepotIndex = depotIndex,
    };
  }

  protected void DeallocateTeam(MedicTeamId teamId, Interval oldShift)
  {
    Moves.Count = 1;
    Moves.Inverse.MovesBuffer[0] = new Move
    {
      Type = MoveType.TeamAllocation,
      DepotIndex = teamId.DepotIndex,
      Shift = oldShift
    };

    Moves.Normal.MovesBuffer[0] = new Move
    {
      Type = MoveType.TeamDeallocation,
      DepotIndex = teamId.DepotIndex,
      OnDepotIndex = teamId.OnDepotIndex
    };
  }

  protected void DeallocateAmbulance(int depotIndex)
  {
    Moves.Count = 1;
    Moves.Inverse.MovesBuffer[0] = new Move
    {
      Type = MoveType.AmbulanceAllocation,
      DepotIndex = depotIndex
    };

    Moves.Normal.MovesBuffer[0] = new Move
    {
      Type = MoveType.AmbulanceDeallocation,
      DepotIndex = depotIndex
    };
  }
}

