namespace ESSP.DataModel;

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


