namespace ESSP.DataModel;

public readonly struct IncidentOpt
{
  public CoordinateOpt Location { get; init; }
  public int OccurenceSec { get; init; }
  public int OnSceneDurationSec { get; init; }
  public int InHospitalDeliverySec { get; init; }
  public IncidentTypeOpt Type { get; init; }
}

