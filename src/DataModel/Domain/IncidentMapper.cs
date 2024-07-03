namespace ESSP.DataModel;

/// Incident mapper

public static class IncidentMapper
{
  private static readonly CoordinateMapper _coordinateMapper = new();

  public static IncidentModel Map(Incident incident)
  {
    return new IncidentModel
    {
      Location = _coordinateMapper.Map(incident.Location),
      OccurenceSec = incident.OccurenceSec,
      OnSceneDurationSec = incident.OnSceneDurationSec,
      InHospitalDeliverySec = incident.InHospitalDeliverySec,
      GoldTimeSec = incident.GoldTimeSec
    };
  }

  public static Incident MapBack(IncidentModel model)
  {
    return new Incident
    {
      Location = _coordinateMapper.MapBack(model.Location),
      OccurenceSec = model.OccurenceSec,
      OnSceneDurationSec = model.OnSceneDurationSec,
      InHospitalDeliverySec = model.InHospitalDeliverySec,
      GoldTimeSec = model.GoldTimeSec
    };
  }
}


