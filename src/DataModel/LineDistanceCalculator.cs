using System;
using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace DataModel;

public class LineDistanceCalculator : IDistanceCalculator
{
  /// average speed of the ambulance
  public int SpeedMetPerSec { get; set; } = 80.ToKmPerHour().Value;

  private readonly ImmutableArray<Hospital> _hospitals;

  public LineDistanceCalculator(ImmutableArray<Hospital> hospitals, int? speedMetPerSec = null)
  {
    _hospitals = hospitals;
    SpeedMetPerSec = speedMetPerSec ?? SpeedMetPerSec;
  }

  /// <returns>Nearest hospital by arrival time, not by pure distance. </returns>
  public Hospital GetNearestHospital(Coordinate location)
  {
    int index = 0;
    int shortestTravelDurationSec = GetTravelDurationSec(_hospitals[0].Location, location);

    for (int i = 1; i < _hospitals.Length; ++i)
    {
      int travelDurationSec = GetTravelDurationSec(_hospitals[i].Location, location);
      if (travelDurationSec < shortestTravelDurationSec)
      {
        shortestTravelDurationSec = travelDurationSec;
        index = i;
      }
    }

    // returns reference, not a copy (array indexing)
    return _hospitals[index];
  }

  public Coordinate GetIntermediateLocation(Coordinate from, Coordinate to, int durationDrivingSec)
  {
    int traveledDistanceMet = SpeedMetPerSec * durationDrivingSec;
    int distanceMet = GetDistanceMet(from, to);

    // crop it so it can't move behind the destination
    if (traveledDistanceMet > distanceMet)
    {
      return to;
    }
    //

    // angle formula
    double angle = Math.Atan2(to.Longitude - from.Longitude, to.Latitude - from.Latitude);
    //

    // calculate new location on the line based on lines angle and duration of the travel
    // calculates in centimeter precision, but result is in meters
    return new Coordinate
    {
      Latitude = (int)(from.Latitude + traveledDistanceMet * Math.Cos(angle)),
      Longitude = (int)(from.Longitude + traveledDistanceMet * Math.Sin(angle))
    };
  }

  /// Calculates travel duration by straight line
  public int GetTravelDurationSec(Coordinate from, Coordinate to)
  {
    return GetDistanceMet(from, to) / SpeedMetPerSec;
  }

  /// returns distance by straight line
  private int GetDistanceMet(Coordinate x, Coordinate y)
  {
    return EuclidianMet(x, y);
  }

  /// classic euclidian distance obtained by pythagoreon theorem
  private int EuclidianMet(Coordinate x, Coordinate y)
  {
    double xLen = Meters.DistanceMet(x.Latitude, y.Latitude);
    double yLen = Meters.DistanceMet(x.Longitude, y.Longitude);

    return (int)Math.Sqrt(xLen * xLen + yLen * yLen);
  }
}