namespace ESSP.DataModel;

public readonly struct Depot
{
  public Coordinate Location { get; }
  public Ambulance[] Ambulances { get; }

  /// sets ambulances locations to passed location
  public Depot(Coordinate location, Ambulance[] ambulances)
  {
    Location = location;
    Ambulances = new Ambulance[ambulances.Length];

    for (int i = 0; i < ambulances.Length; ++i)
    {
      Ambulances[i] = new Ambulance
      {
        Type = ambulances[i].Type,
        Location = location
      };
    }
  }
}

