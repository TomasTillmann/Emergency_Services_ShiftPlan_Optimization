using System.Collections.Immutable;
using System.Linq;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public static class WorldMapper
{
  private static readonly DepotMapper _depotMapper = new();
  private static readonly HospitalMapper _hospitalMapper = new();
  private static readonly AmbulanceTypeMapper _ambulanceTypeMapper = new();

  public static WorldModel Map(World world)
  {
    return new WorldModel
    {
      Depots = world.Depots.Select(depot => _depotMapper.Map(depot)).ToList(),
      Hospitals = world.Hospitals.Select(hospital => _hospitalMapper.Map(hospital)).ToList(),
      AvailableMedicTeams = world.AvailableMedicTeams.ToList(),
      AvailableAmbulances = world.AvailableAmbulances.ToList(),
      AvailableAmbulanceTypes = world.AvailableAmbulanceTypes.Select(type => _ambulanceTypeMapper.Map(type)).ToList(),
      GoldenTimeSec = world.GoldenTimeSec
    };
  }

  public static World MapBack(WorldModel model)
  {
    var hospitals = model.Hospitals.Select(hospital => _hospitalMapper.MapBack(hospital)).ToImmutableArray();
    return new World
    {
      Depots = model.Depots.Select(depot => _depotMapper.MapBack(depot)).ToImmutableArray(),
      Hospitals = hospitals,
      DistanceCalculator = new DistanceCalculator(hospitals.ToArray()),
      AvailableMedicTeams = model.AvailableMedicTeams.ToImmutableArray(),
      AvailableAmbulances = model.AvailableAmbulances.ToImmutableArray(),
      AvailableAmbulanceTypes = model.AvailableAmbulanceTypes.Select(type => _ambulanceTypeMapper.MapBack(type)).ToImmutableArray(),
      GoldenTimeSec = model.GoldenTimeSec
    };
  }
}


