using ESSP.DataModel;

namespace Optimizing;

public interface IMoveGenerator
{
  int MovesBufferSize { get; }
  IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan);
}

