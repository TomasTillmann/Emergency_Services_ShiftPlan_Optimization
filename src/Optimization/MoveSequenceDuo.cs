namespace Optimizing;

public class MoveSequenceDuo
{
  public int Count { get; set; }
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


