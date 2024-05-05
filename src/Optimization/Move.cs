using ESSP.DataModel;

namespace Optimizing;

public readonly struct Move
{
  public int DepotIndex { get; init; } = -1;
  public int OnDepotIndex { get; init; } = -1;
  public int AmbulanceTypeIndex { get; init; } = -1;
  public MoveType MoveType { get; init; } = MoveType.NoMove;

  public Move() { }

  public static Move Identity = new Move { OnDepotIndex = -1 /* The specific index doesn't matter. */ , MoveType = MoveType.NoMove };

  public static bool operator !=(in Move a, in Move b) => !(a == b);

  public static bool operator ==(in Move a, in Move b) =>
    a.DepotIndex == b.DepotIndex && a.OnDepotIndex == b.OnDepotIndex && a.MoveType == b.MoveType;

  public override string ToString()
  {
    return $"({MoveType}, {DepotIndex}, {OnDepotIndex}{(MoveType == MoveType.AllocateAmbulance ? $", {AmbulanceTypeIndex}" : "")})";
  }
}
