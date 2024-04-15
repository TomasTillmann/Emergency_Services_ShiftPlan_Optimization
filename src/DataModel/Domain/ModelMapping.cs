using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DataModel.Interfaces;

namespace ESSP.DataModel;

/// MODEL

/// Incidents

public record IncidentModel
{
  public CoordinateModel Location { get; set; }
  public int OccurenceSec { get; set; }
  public int OnSceneDurationSec { get; set; }
  public int InHospitalDeliverySec { get; set; }
}

/// World
public record WorldModel
{
  public List<DepotModel> Depots { get; set; }
  public List<HospitalModel> Hospitals { get; set; }
  public List<MedicTeam> AvailableMedicTeams { get; set; }
  public List<Ambulance> AvailableAmbulances { get; set; }
  public int GoldenTimeSec { get; set; }
}

public record DepotModel
{
  public CoordinateModel Location { get; set; }
  public List<AmbulanceModel> Ambulances { get; set; }
}

public record AmbulanceModel
{
}

public record HospitalModel
{
  public CoordinateModel Location { get; set; }
}

public record CoordinateModel
{
  public int XMet { get; set; }
  public int YMet { get; set; }
}

/// Incident mapper

public class IncidentMapper
{
  private readonly CoordinateMapper _coordinateMapper = new();

  public IncidentModel Map(Incident incident)
  {
    return new IncidentModel
    {
      Location = _coordinateMapper.Map(incident.Location),
      OccurenceSec = incident.OccurenceSec,
      OnSceneDurationSec = incident.OnSceneDurationSec,
      InHospitalDeliverySec = incident.InHospitalDeliverySec,
    };
  }

  public Incident MapBack(IncidentModel model)
  {
    return new Incident
    {
      Location = _coordinateMapper.MapBack(model.Location),
      OccurenceSec = model.OccurenceSec,
      OnSceneDurationSec = model.OnSceneDurationSec,
      InHospitalDeliverySec = model.InHospitalDeliverySec,
    };
  }
}

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

public class DepotMapper
{
  private readonly AmbulanceMapper _ambulanceMapper = new();
  private readonly CoordinateMapper _coordinateMapper = new();

  public DepotModel Map(Depot depot)
  {
    return new DepotModel
    {
      Location = _coordinateMapper.Map(depot.Location),
    };
  }

  public Depot MapBack(DepotModel model)
  {
    return new Depot
    {
      Location = _coordinateMapper.MapBack(model.Location),
    };
  }
}

public class AmbulanceMapper
{
  private readonly CoordinateMapper _coordinateMapper = new();

  public AmbulanceModel Map(Ambulance ambulance)
  {
    return new AmbulanceModel();
  }

  public Ambulance MapBack(AmbulanceModel model)
  {
    return new Ambulance();
  }
}

public class HospitalMapper
{
  private readonly CoordinateMapper _coordinateMapper = new();

  public HospitalModel Map(Hospital hospital)
  {
    return new HospitalModel
    {
      Location = _coordinateMapper.Map(hospital.Location)
    };
  }

  public Hospital MapBack(HospitalModel model)
  {
    return new Hospital
    {
      Location = _coordinateMapper.MapBack(model.Location)
    };
  }
}

public class CoordinateMapper
{
  public CoordinateModel Map(Coordinate coordinate)
  {
    return new CoordinateModel
    {
      XMet = coordinate.XMet,
      YMet = coordinate.YMet
    };
  }

  public Coordinate MapBack(CoordinateModel model)
  {
    return new Coordinate
    {
      XMet = model.XMet,
      YMet = model.YMet
    };
  }
}

