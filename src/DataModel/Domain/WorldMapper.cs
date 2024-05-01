using System.Collections.Immutable;
using System.Linq;
using DataModel.Interfaces;

namespace ESSP.DataModel;

/// World mapper

public class WorldMapper
{
  private readonly DepotMapper _depotMapper = new();
  private readonly HospitalMapper _hospitalMapper = new();

  public WorldModel Map(World world)
  {
    return new WorldModel
    {
      Depots = world.Depots.Select(depot => _depotMapper.Map(depot)).ToList(),
      Hospitals = world.Hospitals.Select(hospital => _hospitalMapper.Map(hospital)).ToList(),
      AvailableMedicTeams = world.AvailableMedicTeams.ToList(),
      AvailableAmbulances = world.AvailableAmbulances.ToList(),
      GoldenTimeSec = world.GoldenTimeSec
    };
  }

  public World MapBack(WorldModel model)
  {
    var hospitals = model.Hospitals.Select(hospital => _hospitalMapper.MapBack(hospital)).ToImmutableArray();
    return new World
    {
      Depots = model.Depots.Select(depot => _depotMapper.MapBack(depot)).ToImmutableArray(),
      Hospitals = hospitals,
      DistanceCalculator = new DistanceCalculator(hospitals.ToArray()),
      AvailableMedicTeams = model.AvailableMedicTeams.ToImmutableArray(),
      AvailableAmbulances = model.AvailableAmbulances.ToImmutableArray(),
      GoldenTimeSec = model.GoldenTimeSec
    };
  }
}


