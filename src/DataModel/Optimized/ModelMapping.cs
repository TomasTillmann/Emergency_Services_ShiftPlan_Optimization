using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DataModel.Interfaces;

namespace ESSP.DataModel;

/// MODEL

/// Incidents

public class IncidentModel
{
  public CoordinateModel Location { get; set; }
  public int OccurenceSec { get; set; }
  public int OnSceneDurationSec { get; set; }
  public int InHospitalDeliverySec { get; set; }
  public IncidentTypeModel Type { get; set; }
}

public class IncidentTypeModel
{
  public string Name { get; set; }
  public int MaximumResponseTimeSec { get; set; }
}

/// World

public class WorldModel
{
  public List<DepotModel> Depots { get; set; }
  public List<HospitalModel> Hospitals { get; set; }
  public Dictionary<string, HashSet<string>> Table { get; set; }
}

public class DepotModel
{
  public CoordinateModel Location { get; set; }
  public List<AmbulanceModel> Ambulances { get; set; }
}

public class AmbulanceModel
{
  public AmbulanceTypeModel Type { get; set; }
}

public class AmbulanceTypeModel
{
  public string Name { get; set; }
  public int Cost { get; set; }
}

public class HospitalModel
{
  public CoordinateModel Location { get; set; }
}

public class CoordinateModel
{
  public int XMet { get; set; }
  public int YMet { get; set; }
}

/// MAPPERS

/// Incident mapper

public class IncidentModelMapper
{
  private readonly CoordinateModelMapper _coordinateModelMapper = new();
  private readonly IncidentTypeModelMapper _incidentTypeModelMapper = new();

  public IncidentModel Map(IncidentOpt incident)
  {
    return new IncidentModel
    {
      Location = _coordinateModelMapper.Map(incident.Location),
      OccurenceSec = incident.OccurenceSec,
      OnSceneDurationSec = incident.OnSceneDurationSec,
      InHospitalDeliverySec = incident.InHospitalDeliverySec,
      Type = _incidentTypeModelMapper.Map(incident.Type)
    };
  }

  public IncidentOpt MapBack(IncidentModel model)
  {
    return new IncidentOpt
    {
      Location = _coordinateModelMapper.MapBack(model.Location),
      OccurenceSec = model.OccurenceSec,
      OnSceneDurationSec = model.OnSceneDurationSec,
      InHospitalDeliverySec = model.InHospitalDeliverySec,
      Type = _incidentTypeModelMapper.MapBack(model.Type)
    };
  }
}

public class IncidentTypeModelMapper
{
  public IncidentTypeModel Map(IncidentTypeOpt type)
  {
    return new IncidentTypeModel
    {
      Name = type.Name,
      MaximumResponseTimeSec = type.MaximumResponseTimeSec
    };
  }

  public IncidentTypeOpt MapBack(IncidentTypeModel model)
  {
    return new IncidentTypeOpt
    {
      Name = model.Name,
      MaximumResponseTimeSec = model.MaximumResponseTimeSec
    };
  }
}

/// World mapper

public class WorldModelMapper
{
  private readonly DepotModelMapper _depotModelMapper = new();
  private readonly HospitalModelMapper _hospitalModelMapper = new();

  public WorldModel Map(WorldOpt world)
  {
    return new WorldModel
    {
      Depots = world.Depots.Select(depot => _depotModelMapper.Map(depot)).ToList(),
      Hospitals = world.Hospitals.Select(hospital => _hospitalModelMapper.Map(hospital)).ToList(),
      Table = world.IncTypeToAllowedAmbTypesTable.GetTable()
    };
  }

  public WorldOpt MapBack(WorldModel model)
  {
    var hospitals = model.Hospitals.Select(hospital => _hospitalModelMapper.MapBack(hospital)).ToImmutableArray();
    return new WorldOpt
    {
      Depots = model.Depots.Select(depot => _depotModelMapper.MapBack(depot)).ToImmutableArray(),
      Hospitals = hospitals,
      DistanceCalculator = new DistanceCalculatorOpt(hospitals.ToArray()),
      IncTypeToAllowedAmbTypesTable = new IncTypeToAllowedAmbTypesTable(model.Table)
    };
  }
}

public class DepotModelMapper
{
  private readonly AmbulanceModelMapper _ambulanceModelMapper = new();
  private readonly CoordinateModelMapper _coordinateModelMapper = new();

  public DepotModel Map(DepotOpt depot)
  {
    return new DepotModel
    {
      Location = _coordinateModelMapper.Map(depot.Location),
      Ambulances = depot.Ambulances.Select(amb => _ambulanceModelMapper.Map(amb)).ToList()
    };
  }

  public DepotOpt MapBack(DepotModel model)
  {
    return new DepotOpt(
      location: _coordinateModelMapper.MapBack(model.Location),
      ambulances: model.Ambulances.Select(amb => _ambulanceModelMapper.MapBack(amb)).ToArray()
    );
  }
}

public class AmbulanceModelMapper
{
  private readonly AmbulanceTypeModelMapper _ambulanceTypeModelMapper = new();
  private readonly CoordinateModelMapper _coordinateModelMapper = new();

  public AmbulanceModel Map(AmbulanceOpt ambulance)
  {
    return new AmbulanceModel
    {
      Type = _ambulanceTypeModelMapper.Map(ambulance.Type),
    };
  }

  public AmbulanceOpt MapBack(AmbulanceModel model)
  {
    return new AmbulanceOpt
    {
      Type = _ambulanceTypeModelMapper.MapBack(model.Type),
    };
  }
}

public class AmbulanceTypeModelMapper
{
  public AmbulanceTypeModel Map(AmbulanceTypeOpt type)
  {
    return new AmbulanceTypeModel
    {
      Cost = type.Cost,
      Name = type.Name
    };
  }

  public AmbulanceTypeOpt MapBack(AmbulanceTypeModel model)
  {
    return new AmbulanceTypeOpt
    {
      Cost = model.Cost,
      Name = model.Name
    };
  }
}

public class HospitalModelMapper
{
  private readonly CoordinateModelMapper _coordinateModelMapper = new();

  public HospitalModel Map(HospitalOpt hospital)
  {
    return new HospitalModel
    {
      Location = _coordinateModelMapper.Map(hospital.Location)
    };
  }

  public HospitalOpt MapBack(HospitalModel model)
  {
    return new HospitalOpt
    {
      Location = _coordinateModelMapper.MapBack(model.Location)
    };
  }
}

public class CoordinateModelMapper
{
  public CoordinateModel Map(CoordinateOpt coordinate)
  {
    return new CoordinateModel
    {
      XMet = coordinate.XMet,
      YMet = coordinate.YMet
    };
  }

  public CoordinateOpt MapBack(CoordinateModel model)
  {
    return new CoordinateOpt
    {
      XMet = model.XMet,
      YMet = model.YMet
    };
  }
}

