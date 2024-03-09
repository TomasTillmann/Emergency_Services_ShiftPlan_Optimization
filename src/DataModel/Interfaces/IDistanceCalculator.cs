using System.Collections.Generic;
using System.Collections.Immutable;
using ESSP.DataModel;

namespace DataModel.Interfaces;

public interface IDistanceCalculator
{
  IEnumerable<T> GetNearestLocatable<T>(ILocatable locatable, IEnumerable<T> locatables) where T : ILocatable;

  Coordinate GetNewLocation(Coordinate from, Coordinate to, Seconds duration, Seconds currentTime);

  Seconds GetTravelDuration(ILocatable from, ILocatable to, Seconds currentTime);

  Seconds GetTravelDuration(Coordinate from, Coordinate to, Seconds currentTime);
}

