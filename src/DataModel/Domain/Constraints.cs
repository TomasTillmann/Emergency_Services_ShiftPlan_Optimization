using System.Collections.Generic;

namespace ESSP.DataModel;

public class Constraints
{
  public HashSet<int> AllowedShiftStartingTimesSec { get; init; } = new();

  public HashSet<int> AllowedShiftDurationsSec { get; init; } = new();
}
