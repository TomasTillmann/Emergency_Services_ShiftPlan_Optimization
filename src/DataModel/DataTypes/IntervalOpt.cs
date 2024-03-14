using System;

namespace ESSP.DataModel
{
  public readonly struct IntervalOpt
  {
    public int StartSec { get; init; }
    public int EndSec { get; init; }
    public int Duration => EndSec - StartSec;

    public static IntervalOpt Empty = new IntervalOpt { StartSec = 0, EndSec = 0 };

    public static IntervalOpt GetByStartAndDuration(int startSec, int durationSec)
    {
      return new IntervalOpt
      {
        StartSec = startSec,
        EndSec = startSec + durationSec
      };
    }

    public static IntervalOpt GetByStartAndEnd(int startSec, int endSec)
    {
      return new IntervalOpt
      {
        StartSec = startSec,
        EndSec = endSec
      };
    }

    public static IntervalOpt GetUnion(IntervalOpt[] intervals)
    {
      int startSec = int.MaxValue;
      int endSec = int.MinValue;

      for (int i = 0; i < intervals.Length; ++i)
      {
        IntervalOpt interval = intervals[i];
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

    public bool IsSubsetOf(IntervalOpt interval)
    {
      return interval.IsInInterval(StartSec) && interval.IsInInterval(EndSec);
    }

    public static bool operator ==(IntervalOpt i1, IntervalOpt i2) => i1.StartSec == i2.StartSec && i1.EndSec == i2.EndSec;
    public static bool operator !=(IntervalOpt i1, IntervalOpt i2) => i1.StartSec != i2.StartSec || i1.EndSec != i2.EndSec;

    public override bool Equals(object obj)
    {
      throw new InvalidOperationException("I never want to use, it's slow.");
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(StartSec.GetHashCode(), EndSec.GetHashCode());
    }
  }
}

