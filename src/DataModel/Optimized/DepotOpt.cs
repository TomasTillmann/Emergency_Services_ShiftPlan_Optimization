namespace ESSP.DataModel;

public readonly struct DepotOpt
{
  public CoordinateOpt Location { get; }
  public AmbulanceOpt[] Ambulances { get; }

  /// sets ambulances locations to passed location
  public DepotOpt(CoordinateOpt location, AmbulanceOpt[] ambulances)
  {
    Location = location;
    Ambulances = new AmbulanceOpt[ambulances.Length];

    for (int i = 0; i < ambulances.Length; ++i)
    {
      Ambulances[i] = new AmbulanceOpt
      {
        Type = ambulances[i].Type,
        Location = location
      };
    }
  }
}

