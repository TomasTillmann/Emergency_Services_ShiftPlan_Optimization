using System.Collections.Immutable;
using System.Linq;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public class World
{
  public ImmutableArray<Depot> Depots { get; init; }
  public ImmutableArray<Hospital> Hospitals { get; init; }
  public IDistanceCalculator DistanceCalculator { get; init; }
  public ImmutableArray<MedicTeam> AvailableMedicTeams { get; init; }
  public ImmutableArray<Ambulance> AvailableAmbulances { get; init; }
}
