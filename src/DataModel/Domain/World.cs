using DataModel.Interfaces;
using System.Collections.Generic;

namespace ESSP.DataModel;

public class World
{
    public IReadOnlyList<Depot> Depots { get; set; }

    public IReadOnlyList<Hospital> Hospitals { get; set; }

    public World(IReadOnlyList<Depot> depots, IReadOnlyList<Hospital> hospitals)
    {
        Depots = depots;
        Hospitals = hospitals;
    }
}
