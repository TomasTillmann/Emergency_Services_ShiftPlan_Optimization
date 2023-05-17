using Newtonsoft.Json;
using System.Collections.Generic;

namespace ESSP.DataModel
{
    public readonly struct Interval
    {
        public Seconds Start { get; init; }

        public Seconds End { get; init; }

        public Seconds Duration => End - Start;

        private Interval(Seconds start, Seconds end)
        {
            Start = start;
            End = end;
        }

        public static Interval GetByStartAndDuration(Seconds start, Seconds duration)
        {
            return new Interval(start, start + duration);
        }

        public static Interval GetByStartAndDurationFromSeconds(int start, int duration)
        {
            return new Interval(start.ToSeconds(), (start + duration).ToSeconds());
        }

        public static Interval GetByStartAndEnd(Seconds start, Seconds end)
        {
            return new Interval(start, end);
        }

        public static Interval GetUnion(IEnumerable<Interval> intervals)
        {
            Seconds start = Seconds.MaxValue;
            Seconds end = Seconds.MinValue;

            foreach (Interval interval in intervals)
            {
                if (interval.Start < start)
                {
                    start = interval.Start;
                }

                if (interval.End > end)
                {
                    end = interval.End;
                }
            }

            return GetByStartAndEnd(start, end);
        }

        public bool Contains(Seconds time)
        {
            return Start <= time && time <= End;
        }

        public bool IsSubsetOf(Interval interval)
        {
            return interval.Contains(Start) && interval.Contains(End);
        }

        public override string ToString()
        {
            return $"INTERVAL: {{ {Start} : {End} }}";
        }
    }
}
