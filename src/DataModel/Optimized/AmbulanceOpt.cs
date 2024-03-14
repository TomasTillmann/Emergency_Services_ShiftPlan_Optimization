namespace ESSP.DataModel;

public readonly struct AmbulanceOpt
{
  public static int ReroutePenaltySec { get; set; } = 30;

  public AmbulanceTypeOpt Type { get; init; }
  public CoordinateOpt Location { get; init; }
}

