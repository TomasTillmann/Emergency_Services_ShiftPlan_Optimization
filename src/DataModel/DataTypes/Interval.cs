using System;

namespace ESSP.DataModel
{
  public readonly struct Interval
  {
    public int StartSec { get; init; }
    public int EndSec { get; init; }
    public int DurationSec => EndSec - StartSec;

    public static Interval Empty = new Interval { StartSec = 0, EndSec = 0 };

    public static Interval GetByStartAndDuration(int startSec, int durationSec)
    {
      return new Interval
      {
        StartSec = startSec,
        EndSec = startSec + durationSec
      };
    }

    public static Interval GetByStartAndEnd(int startSec, int endSec)
    {
      return new Interval
      {
        StartSec = startSec,
        EndSec = endSec
      };
    }

    public static Interval GetUnion(Interval[] intervals)
    {
      int startSec = int.MaxValue;
      int endSec = int.MinValue;

      for (int i = 0; i < intervals.Length; ++i)
      {
        Interval interval = intervals[i];
        if (interval.StartSec < startSec)
        {
          startSec = interval.StartSec;
        }

        if (interval.EndSec > endSec)
        {
          endSec = interval.EndSec;
        }
      }

      return GetByStartAndEnd(startSec, endSec);
    }

    public bool IsInInterval(int timeSec)
    {
      return StartSec <= timeSec && timeSec <= EndSec;
    }

    public bool IsSubsetOf(Interval interval)
    {
      return interval.IsInInterval(StartSec) && interval.IsInInterval(EndSec);
    }

    public static bool operator ==(Interval i1, Interval i2) => i1.StartSec == i2.StartSec && i1.EndSec == i2.EndSec;
    public static bool operator !=(Interval i1, Interval i2) => i1.StartSec != i2.StartSec || i1.EndSec != i2.EndSec;

    public override bool Equals(object obj)
    {
      if (obj is Interval interval)
      {
        return this == interval;
      }

      return false;
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(StartSec.GetHashCode(), EndSec.GetHashCode());
    }
  }
}

