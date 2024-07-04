using ESSP.DataModel;

namespace Optimizing;

public abstract class MoveGeneratorBase : IMoveGenerator
{
  public int MovesBufferSize { get; }
  protected ShiftTimes ShiftTimes { get; set; }
  protected Constraints Constraints { get; set; }
  protected MoveSequenceDuo Moves { get; set; }

  public MoveGeneratorBase(ShiftTimes shiftTimes, Constraints constraints, int movesBufferSize)
  {
    ShiftTimes = shiftTimes;
    Constraints = constraints;
    MovesBufferSize = movesBufferSize;
    Moves = new MoveSequenceDuo
    {
      Normal = new MoveSequence(movesBufferSize),
      Inverse = new MoveSequence(movesBufferSize)
    };
  }

  public abstract IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan);
}

