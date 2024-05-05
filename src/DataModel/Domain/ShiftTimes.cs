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

      List<int> durations = value.ToList();
      durations.Add(0);
      _allowedDurationsSecSortedIncludingZero = durations.OrderBy(duration => duration).ToArray();
    }
  }

  private readonly int[] _allowedDurationsSecSortedIncludingZero;

  public int GetRandomStartingTimeSec(Random random = null)
  {
    random ??= new Random();
    return AllowedStartingTimesSecSorted.ElementAt(random.Next(0, AllowedShiftStartingTimesSec.Count));
  }

  /// <summary>
  /// Including 0 duration.
  /// </summary>
  public int GetRandomDurationTimeSec(Random random = null)
  {
    random ??= new Random();
    return _allowedDurationsSecSortedIncludingZero.ElementAt(random.Next(0, _allowedDurationsSecSortedIncludingZero.Length));
  }
}
