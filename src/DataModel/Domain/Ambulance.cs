namespace ESSP.DataModel;

public readonly struct Ambulance
{
  public static int ReroutePenaltySec { get; set; } = 30;

  public AmbulanceType Type { get; init; }
  public Coordinate Location { get; init; }
}

