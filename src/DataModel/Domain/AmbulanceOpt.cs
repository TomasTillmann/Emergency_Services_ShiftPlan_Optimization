namespace ESSP.DataModel;

public readonly struct AmbulanceOpt
{
  public AmbulanceTypeOpt Type { get; init; }
  public int ReroutePenaltySec { get; init; }
  public CoordinateOpt Location { get; init; }
}

