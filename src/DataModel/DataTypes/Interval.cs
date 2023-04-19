using System.Collections.Generic;

namespace ESSP.DataModel
{
    public readonly struct Interval
    {
        public Seconds Start { get; }
        public Seconds End { get; }
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

        public override string ToString()
        {
            return $"INTERVAL: {{ {Start} : {End} }}";
        }
    }
}
