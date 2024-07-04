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

  public MoveSequenceDuo(int maxMovesSize)
  {
    Normal = new MoveSequence(maxMovesSize);
    Inverse = new MoveSequence(maxMovesSize);
  }
  
  public MoveSequenceDuo() {}

  public override string ToString()
  {
    return $"Normal: {Normal.ToString()}\nInverse: {Inverse.ToString()}";
  }
}


