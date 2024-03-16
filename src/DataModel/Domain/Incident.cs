namespace ESSP.DataModel;

public readonly struct Incident
{
  public Coordinate Location { get; init; }
  public int OccurenceSec { get; init; }
  public int OnSceneDurationSec { get; init; }
  public int InHospitalDeliverySec { get; init; }
  public IncidentType Type { get; init; }
}

