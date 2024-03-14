using System;
using System.Collections.Immutable;
using ESSP.DataModel;

namespace DataModel.Interfaces;

public class DistanceCalculatorOpt
{
  /// average speed of the ambulance
  public static int SpeedMetPerSec { get; } = 80.ToKmPerHour().Value;

  private ImmutableArray<HospitalOpt> _hospitals;

  public DistanceCalculatorOpt(HospitalOpt[] hospitals)
  {
    Array.Sort(hospitals, (x, y) => x.Location.CompareTo(y.Location));
    _hospitals = hospitals.ToImmutableArray();
  }

  /// <returns>Nearest hospital by arrival time, not by pure distance. </returns>
  public HospitalOpt GetNearestHospital(CoordinateOpt location)
  {
    // It could be found in a binary search manner for each coordinate, but since there is not many hospitals (usually around hundreads),
    // simple traverse is enough (if not better)
    // TODO: Precalculate?

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

  public CoordinateOpt GetNewLocation(CoordinateOpt from, CoordinateOpt to, int durationDrivingSec, int firstPossibleStartTimeSec)
  {
    int traveledDistanceMet = DistanceCalculatorOpt.SpeedMetPerSec * durationDrivingSec;
    int distanceMet = GetDistanceMet(from, to);

    // crop it so it can't move behind the destination
    if (traveledDistanceMet > distanceMet)
    {
      return to;
    }
    //

    // angle formula
    double angle = Math.Atan2(to.YMet - from.YMet, to.XMet - from.XMet);
    //

    // calculate new location on the line based on lines angle and duration of the travel
    // calculates in centimeter precision, but result is in meters
    return new CoordinateOpt
    {
      XMet = (int)(from.XMet + traveledDistanceMet * Math.Cos(angle)),
      YMet = (int)(from.YMet + traveledDistanceMet * Math.Sin(angle))
    };
  }

  /// Calculates travel duration by straight line
  public int GetTravelDurationSec(CoordinateOpt from, CoordinateOpt to)
  {
    return GetDistanceMet(from, to) / DistanceCalculatorOpt.SpeedMetPerSec;
  }

  /// returns distance by straight line
  private int GetDistanceMet(CoordinateOpt x, CoordinateOpt y)
  {
    return EuclidianMet(x, y);
  }

  /// classic euclidian distance obtained by pythagoreon theorem
  private int EuclidianMet(CoordinateOpt x, CoordinateOpt y)
  {
    long xLen = Meters.DistanceMet(x.XMet, y.XMet);
    long yLen = Meters.DistanceMet(x.YMet, y.YMet);

    return (int)Math.Sqrt(xLen * xLen + yLen * yLen);
  }

}

