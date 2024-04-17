namespace Optimizing;

public readonly struct Move
{
  public int DepotIndex { get; init; }
  public int MedicTeamOnDepotIndex { get; init; }
  public MoveType MoveType { get; init; }

  public static Move Identity = new Move { MedicTeamOnDepotIndex = -1 /* The specific index doesn't matter. */ , MoveType = MoveType.NoMove };

  public override string ToString()
  {
    return $"({MoveType}, {DepotIndex}, {MedicTeamOnDepotIndex})";
  }
}
