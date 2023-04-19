using System.Collections.Generic;
using ESSP.DataModel;

namespace DataModel.Interfaces;

public interface IDistanceCalculator
{
    Seconds GetTravelDuration(ILocatable from, ILocatable to, Seconds currentTime);

    Coordinate GetNewLocation(ILocatable from, Seconds duration, Seconds currentTime);

    IEnumerable<T> GetNearestLocatable<T>(ILocatable locatable, IEnumerable<T> locatables) where T : ILocatable;
}