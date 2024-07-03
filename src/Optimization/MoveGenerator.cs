using ESSP.DataModel;

namespace Optimizing;

public abstract class MoveGeneratorBase : IMoveGenerator
{
  protected ShiftTimes ShiftTimes { get; set; }
  protected Constraints Constraints { get; set; }
  protected MoveSequenceDuo Moves { get; set; }

  public MoveGeneratorBase(ShiftTimes shiftTimes, Constraints constraints, int moveSequenceBuffer)
  {
    ShiftTimes = shiftTimes;
    Constraints = constraints;
    Moves = new MoveSequenceDuo
    {
      Normal = new MoveSequence(moveSequenceBuffer),
      Inverse = new MoveSequence(moveSequenceBuffer)
    };
  }

  public abstract IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan);
}

