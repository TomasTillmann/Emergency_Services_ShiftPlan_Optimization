using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Move generator, by which neighbourhood of an emergency service plan is implicitely defined.
/// </summary>
public interface IMoveGenerator
{
  /// <summary>
  /// Maximal buffer needed for moves.
  /// For example, if move generator allows for sequences of moves eg. allocate team, allocate ambulance,
  /// than it should be set to 2, if this is the maximum allowed move sequence.
  /// </summary>
  int MovesBufferSize { get; }
  
  /// <summary>
  /// Gets all the moves in lazy fashion.
  /// </summary>
  IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan);
}

