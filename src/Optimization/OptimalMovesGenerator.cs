using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class OptimalMovesGenerator : MoveGeneratorBase
{
  public int K { get; set; }
  public ImmutableArray<Incident> Incidents { get; set; }

  public OptimalMovesGenerator(ShiftTimes shiftTimes, Constraints constraints, int movesBufferSize) : base(shiftTimes, constraints, movesBufferSize)
  {
  }

  public override IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan)
  {
    throw new NotImplementedException();
  }
}

