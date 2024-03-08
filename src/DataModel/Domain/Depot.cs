using System.Collections.Generic;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public class Depot : ILocatable
{
  public Coordinate Location { get; set; }

  public IList<Ambulance> Ambulances { get; set; }

  public Depot(Coordinate coordinate, IList<Ambulance> ambulances)
  {
    Location = coordinate;
    Ambulances = ambulances;

    foreach (Ambulance ambulance in Ambulances)
    {
      ambulance.Location = Location;
    }
  }
}
