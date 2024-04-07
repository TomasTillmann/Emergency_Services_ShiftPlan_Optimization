using System.Collections.Immutable;
using System.Linq;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public class World
{
  public ImmutableArray<Depot> Depots { get; set; }
  public ImmutableArray<Hospital> Hospitals { get; set; }
  public DistanceCalculator DistanceCalculator { get; set; }
  public IncTypeToAllowedAmbTypesTable IncTypeToAllowedAmbTypesTable { get; set; }

  private int? _allAmbulancesCount;
  public int AllAmbulancesCount => _allAmbulancesCount ??= Depots.Sum(depot => depot.Ambulances.Count());
}
