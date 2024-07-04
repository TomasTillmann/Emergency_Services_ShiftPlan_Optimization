using ESSP.DataModel;

namespace Optimizing;

public interface IMoveGenerator
{
  IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan);
  int MovesBufferSize { get; }
}


