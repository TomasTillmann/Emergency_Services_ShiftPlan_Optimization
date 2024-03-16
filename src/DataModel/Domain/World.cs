using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public class World
{
  public ImmutableArray<Depot> Depots { get; set; }
  public ImmutableArray<Hospital> Hospitals { get; set; }
  public DistanceCalculator DistanceCalculator { get; set; }
  public IncTypeToAllowedAmbTypesTable IncTypeToAllowedAmbTypesTable { get; set; }
}
