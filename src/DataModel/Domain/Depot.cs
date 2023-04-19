using System;
using System.Collections.Generic;
using DataModel.Interfaces;

namespace ESSP.DataModel;
public class Depot : ILocatable
{
    public Coordinate Location { get; }

    public IList<Ambulance> Ambulances { get; }

    public Depot(Coordinate coordinate, IList<Ambulance> ambulances)
    {
        Location = coordinate;
        Ambulances = ambulances;
    }

}