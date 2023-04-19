using System;
using DataModel.Interfaces;

namespace ESSP.DataModel;
public class Hospital : ILocatable
{
    public Coordinate Location { get; }

    public Hospital(Coordinate coordinate)
    {
        Location = coordinate;
    }
}