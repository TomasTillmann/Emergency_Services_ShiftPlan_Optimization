using ESSP.DataModel;

namespace Optimizing;

public readonly struct Move
{
  public static Move Identity = new Move { Type = MoveType.Identity };

  public MoveType Type { get; init; }
  public Interval Shift { get; init; }
  public int DepotIndex { get; init; }
  public int OnDepotIndex { get; init; }

  public override string ToString()
  {
    return $"({Type}, {Shift}, {DepotIndex}, {OnDepotIndex})";
  }
}

