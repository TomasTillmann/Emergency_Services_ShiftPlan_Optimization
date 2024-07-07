using System;

namespace Optimizing;

public class MoveSequence
{
  public int Count { get; set; }
  public Move[] MovesBuffer { get; init; }

  public static MoveSequence GetNewEmpty(int maxMovesCount) => new MoveSequence(maxMovesCount);

  public static MoveSequence GetNewFrom(MoveSequence other)
  {
    MoveSequence move = GetNewEmpty(other.MovesBuffer.Length);
    move.FillFrom(other);
    return move;
  }

  private MoveSequence(int movesBufferSize)
  {
    MovesBuffer = new Move[movesBufferSize];
  }

  public MoveSequence() { }

  public void FillFrom(MoveSequence other)
  {
    Count = other.Count;
    for (int i = 0; i < Count; ++i)
    {
      MovesBuffer[i] = other.MovesBuffer[i];
    }
  }

  public override string ToString()
  {
    return string.Join(", ", MovesBuffer);
  }

}


