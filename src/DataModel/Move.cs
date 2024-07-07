using System;
using ESSP.DataModel;

namespace Optimizing;

public readonly struct Move
{
  public static Move Identity = new Move { Type = MoveType.Identity };
  public MoveType Type { get; init; }
  public Interval Shift { get; init; }
  public int DepotIndex { get; init; }
  public int OnDepotIndex { get; init; }

  public static bool operator ==(Move x, Move y) => x.Type == y.Type && x.Shift == y.Shift && x.DepotIndex == y.DepotIndex && x.OnDepotIndex == y.OnDepotIndex;
  public static bool operator !=(Move x, Move y) => !(x == y);

  public override string ToString()
  {
    return $"({Type}, {Shift}, {DepotIndex}, {OnDepotIndex})";
  }
}

