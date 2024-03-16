namespace Optimizing;

public readonly struct Move
{
  public int WeightIndex { get; init; }
  public MoveType MoveType { get; init; }

  public static Move Identity = new Move { WeightIndex = 0 /* The specific index doesn't matter. */ , MoveType = MoveType.NoMove };

  public override string ToString()
  {
    return $"({MoveType}, {WeightIndex})";
  }
}
