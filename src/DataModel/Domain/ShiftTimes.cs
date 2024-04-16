using System;
using System.Linq;
using System.Collections.Generic;

namespace ESSP.DataModel;

public class ShiftTimes
{
  public int MinDurationSec { get; init; }
  public int MaxDurationSec { get; init; }
  public int EarliestStartingTimeSec { get; init; }
  public int LatestStartingTimeSec { get; init; }

  public int[] AllowedDurationsSecSorted { get; init; }
  public int[] AllowedStartingTimesSecSorted { get; init; }

  private readonly HashSet<int> _allowedStartingTimesSec;
  public HashSet<int> AllowedShiftStartingTimesSec
  {
    get
    {
      return _allowedStartingTimesSec;
    }
    init
    {
      _allowedStartingTimesSec = value;
      AllowedStartingTimesSecSorted = _allowedStartingTimesSec.ToList().OrderBy(duration => duration).ToArray();
      EarliestStartingTimeSec = AllowedStartingTimesSecSorted.First();
      LatestStartingTimeSec = AllowedStartingTimesSecSorted.Last();
    }
  }


  private readonly HashSet<int> _allowedDurationsSec;
  public HashSet<int> AllowedShiftDurationsSec
  {
    get
    {
      return _allowedDurationsSec;
    }
    init
    {
      _allowedDurationsSec = value;
      AllowedDurationsSecSorted = _allowedDurationsSec.ToList().OrderBy(duration => duration).ToArray();
      MinDurationSec = AllowedDurationsSecSorted.First();
      MaxDurationSec = AllowedDurationsSecSorted.Last();
    }
  }

  public int GetRandomStartingTimeSec(Random random = null)
  {
    random ??= new Random();
    return AllowedStartingTimesSecSorted.ElementAt(random.Next(0, AllowedShiftStartingTimesSec.Count()));
  }

  public int GetRandomDurationTimeSec(Random random = null)
  {
    random ??= new Random();

    // First element is 0 duration. It's ordered by duration.
    return AllowedDurationsSecSorted.ElementAt(random.Next(1, AllowedShiftDurationsSec.Count()));
  }
}
