using System.Collections.Immutable;

namespace ESSP.DataModel;

public class Constraints
{
  public ImmutableArray<int> MaxTeamsPerDepotCount { get; init; }
  public ImmutableArray<int> MaxAmbulancesPerDepotCount { get; init; }
}
