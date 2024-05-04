namespace Optimizing;

public readonly struct Move
{
  public int DepotIndex { get; init; }
  public int OnDepotIndex { get; init; }
  public MoveType MoveType { get; init; }

  public static Move Identity = new Move { OnDepotIndex = -1 /* The specific index doesn't matter. */ , MoveType = MoveType.NoMove };

  public static bool operator !=(in Move a, in Move b) => !(a == b);

  public static bool operator ==(in Move a, in Move b) =>
    a.DepotIndex == b.DepotIndex && a.OnDepotIndex == b.OnDepotIndex && a.MoveType == b.MoveType;

  public override string ToString()
  {
    return $"({MoveType}, {DepotIndex}, {OnDepotIndex})";
  }
}
