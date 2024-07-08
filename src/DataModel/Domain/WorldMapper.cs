using System.Collections.Immutable;
using System.Linq;
using DataModel.Interfaces;
using DistanceAPI;

namespace ESSP.DataModel;

/// World mapper

public static class WorldMapper
{
  private static readonly DepotMapper _depotMapper = new();
  private static readonly HospitalMapper _hospitalMapper = new();
  private static readonly MedicTeamMapper _medicTeamMapper = new();
  private static readonly AmbulanceMapper _ambMapper = new();

  public static WorldModel Map(World world)
  {
    return new WorldModel
    {
      Depots = world.Depots.Select(depot => _depotMapper.Map(depot)).ToList(),
      Hospitals = world.Hospitals.Select(hospital => _hospitalMapper.Map(hospital)).ToList(),
      AvailableMedicTeams = world.AvailableMedicTeams.Select(team => _medicTeamMapper.Map(team)).ToList(),
      AvailableAmbulances = world.AvailableAmbulances.Select(amb => _ambMapper.Map(amb)).ToList(),
    };
  }

  public static World MapBack(WorldModel model)
  {
    var hospitals = model.Hospitals.Select(hospital => _hospitalMapper.MapBack(hospital)).ToImmutableArray();
    return new World
    {
      Depots = model.Depots.Select(depot => _depotMapper.MapBack(depot)).ToImmutableArray(),
      Hospitals = hospitals,
      AvailableMedicTeams = model.AvailableMedicTeams.Select(team => _medicTeamMapper.MapBack(team)).ToImmutableArray(),
      AvailableAmbulances = model.AvailableAmbulances.Select(amb => _ambMapper.MapBack(amb)).ToImmutableArray(),
      DistanceCalculator = new RealDistanceCalculator(hospitals)
    };
  }
}


