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
  public IncidentTypeModel Type { get; set; }
}

public record IncidentTypeModel
{
  public string Name { get; set; }
  public int MaximumResponseTimeSec { get; set; }
}

/// World

public record WorldModel
{
  public List<DepotModel> Depots { get; set; }
  public List<HospitalModel> Hospitals { get; set; }
  public AmbulanceTypeModel[] AmbTypes { get; set; }
  public Dictionary<string, HashSet<string>> IncToAmbTypesTable { get; set; }
}

public record DepotModel
{
  public CoordinateModel Location { get; set; }
  public List<AmbulanceModel> Ambulances { get; set; }
}

public record AmbulanceModel
{
  public AmbulanceTypeModel Type { get; set; }
}

public record AmbulanceTypeModel
{
  public string Name { get; set; }
  public double Cost { get; set; }
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
  private readonly IncidentTypeMapper _incidentTypeMapper = new();

  public IncidentModel Map(Incident incident)
  {
    return new IncidentModel
    {
      Location = _coordinateMapper.Map(incident.Location),
      OccurenceSec = incident.OccurenceSec,
      OnSceneDurationSec = incident.OnSceneDurationSec,
      InHospitalDeliverySec = incident.InHospitalDeliverySec,
      Type = _incidentTypeMapper.Map(incident.Type)
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
      Type = _incidentTypeMapper.MapBack(model.Type)
    };
  }
}

public class IncidentTypeMapper
{
  public IncidentTypeModel Map(IncidentType type)
  {
    return new IncidentTypeModel
    {
      Name = type.Name,
      MaximumResponseTimeSec = type.MaximumResponseTimeSec
    };
  }

  public IncidentType MapBack(IncidentTypeModel model)
  {
    return new IncidentType
    {
      Name = model.Name,
      MaximumResponseTimeSec = model.MaximumResponseTimeSec
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
      AmbTypes = world.AmbTypes,
      IncToAmbTypesTable = world.IncTypeToAllowedAmbTypesTable.GetTable()
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
      AmbTypes = model.AmbTypes,
      IncTypeToAllowedAmbTypesTable = new IncTypeToAllowedAmbTypesTable(model.IncToAmbTypesTable),
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
      Ambulances = depot.Ambulances.Select(amb => _ambulanceMapper.Map(amb)).ToList()
    };
  }

  public Depot MapBack(DepotModel model)
  {
    return new Depot(
      location: _coordinateMapper.MapBack(model.Location),
      ambulances: model.Ambulances.Select(amb => _ambulanceMapper.MapBack(amb)).ToArray()
    );
  }
}

public class AmbulanceMapper
{
  private readonly AmbulanceTypeMapper _ambulanceTypeMapper = new();
  private readonly CoordinateMapper _coordinateMapper = new();

  public AmbulanceModel Map(Ambulance ambulance)
  {
    return new AmbulanceModel
    {
      Type = _ambulanceTypeMapper.Map(ambulance.Type),
    };
  }

  public Ambulance MapBack(AmbulanceModel model)
  {
    return new Ambulance
    {
      Type = _ambulanceTypeMapper.MapBack(model.Type),
    };
  }
}

public class AmbulanceTypeMapper
{
  public AmbulanceTypeModel Map(AmbulanceType type)
  {
    return new AmbulanceTypeModel
    {
      Cost = type.Cost,
      Name = type.Name
    };
  }

  public AmbulanceType MapBack(AmbulanceTypeModel model)
  {
    return new AmbulanceType
    {
      Cost = model.Cost,
      Name = model.Name
    };
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

