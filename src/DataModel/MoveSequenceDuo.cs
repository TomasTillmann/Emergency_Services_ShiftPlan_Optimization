namespace Optimizing;

public class MoveSequenceDuo
{
  private int _count;
  public int Count
  {
    get => _count;
    set
    {
      _count = value;
      Normal.Count = value;
      Inverse.Count = value;
    }
  }
  public MoveSequence Normal { get; set; }
  public MoveSequence Inverse { get; set; }

  public static MoveSequenceDuo GetNewEmpty(int maxMovesSize) => new(maxMovesSize);

  private MoveSequenceDuo(int maxMovesSize)
  {
    Normal = MoveSequence.GetNewEmpty(maxMovesSize);
    Inverse = MoveSequence.GetNewEmpty(maxMovesSize);
  }

  public MoveSequenceDuo() { }

  public void FillFrom(MoveSequenceDuo other)
  {
    Count = other.Count;
    Normal.FillFrom(other.Normal);
    Inverse.FillFrom(other.Inverse);
  }

  public override string ToString()
  {
    return $"Normal: {Normal.ToString()}\nInverse: {Inverse.ToString()}";
  }
}


