using System;
using System.Collections.Generic;
using ESSP.DataModel;
using Model.Extensions;

namespace DataModel.Interfaces;

public interface IDistanceCalculator
{
    IEnumerable<T> GetNearestLocatable<T>(ILocatable locatable, IEnumerable<T> locatables) where T : ILocatable;

    Coordinate GetNewLocation(Coordinate from, Coordinate to, Seconds duration, Seconds currentTime);

    Seconds GetTravelDuration(ILocatable from, ILocatable to, Seconds currentTime);

    Seconds GetTravelDuration(Coordinate from, Coordinate to, Seconds currentTime);
}

public class DistanceCalculator : IDistanceCalculator
{
    private static readonly MetersPerSecond speed = 100.ToKmPerHour(); 


    public IEnumerable<T> GetNearestLocatable<T>(ILocatable locatable, IEnumerable<T> locatables) where T : ILocatable
    {
        return locatables.FindMinSubset((loc) => GetDistance(loc.Location, locatable.Location));
    }

    public Coordinate GetNewLocation(Coordinate from, Coordinate to, Seconds duration, Seconds currentTime)
    {
        // angle formula
        Meters distance = GetDistance(from, to);
        double angle = Math.Atan2(to.Y.Value - from.Y.Value, to.X.Value - from.X.Value);
        //

        // crop it so it can't move behind the destination
        return new Coordinate
        (
            Math.Max((int)(from.X.Value + distance.Value * Math.Cos(angle)), to.X.Value).ToMeters(),
            Math.Max((int)(from.Y.Value + distance.Value * Math.Sin(angle)), to.Y.Value).ToMeters()
        );
    }

    public Seconds GetTravelDuration(ILocatable from, ILocatable to, Seconds currentTime)
    {
        return GetTravelDuration(from.Location, to.Location, currentTime);
    }

    public Seconds GetTravelDuration(Coordinate from, Coordinate to, Seconds currentTime)
    {
        return GetDistance(from, to) / speed;
    }


    private  Meters GetDistance(Coordinate x, Coordinate y)
    {
        return Euclidian(x, y);
    }

    private Meters Euclidian(Coordinate x, Coordinate y)
    {
        long xLen = Meters.Distance(x.X, y.X).Value;
        long yLen = Meters.Distance(x.Y, y.Y).Value;

        return ((int)Math.Sqrt(xLen * xLen + yLen * yLen)).ToMeters();
    }
}