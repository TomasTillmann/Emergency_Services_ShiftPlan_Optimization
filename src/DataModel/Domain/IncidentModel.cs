namespace ESSP.DataModel;

/// MODEL

/// Incidents

public record IncidentModel
{
  public CoordinateModel Location { get; set; }
  public int OccurenceSec { get; set; }
  public int OnSceneDurationSec { get; set; }
  public int InHospitalDeliverySec { get; set; }
  public int GoldTimeSec { get; set; }
}


