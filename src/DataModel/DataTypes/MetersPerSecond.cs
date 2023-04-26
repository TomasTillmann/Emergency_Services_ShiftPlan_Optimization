using System;

namespace ESSP.DataModel
{
    public readonly struct MetersPerSecond : IComparable<MetersPerSecond>, IComparable
    {
        public int Value { get; }

        public MetersPerSecond(int value)
        {
            Value = value;
        }

        public static MetersPerSecond MinValue => 0.ToMetersPerSecond();
        public static MetersPerSecond MaxValue => int.MaxValue.ToMetersPerSecond();

        #region Operators

        public static MetersPerSecond operator +(MetersPerSecond a, MetersPerSecond b) => new MetersPerSecond(a.Value + b.Value);
        public static MetersPerSecond operator -(MetersPerSecond a, MetersPerSecond b) => new MetersPerSecond(a.Value - b.Value);
        public static bool operator <(MetersPerSecond a, MetersPerSecond b) => a.Value < b.Value;
        public static bool operator >(MetersPerSecond a, MetersPerSecond b) => a.Value > b.Value;
        public static bool operator <=(MetersPerSecond a, MetersPerSecond b) => a.Value <= b.Value;
        public static bool operator >=(MetersPerSecond a, MetersPerSecond b) => a.Value >= b.Value;
        public static bool operator ==(MetersPerSecond a, MetersPerSecond b) => a.Value == b.Value;
        public static bool operator !=(MetersPerSecond a, MetersPerSecond b) => a.Value != b.Value;

        // s = v * t
        public static Meters operator *(MetersPerSecond a, Seconds b) => new Meters(a.Value * b.Value);

        #endregion

        public int CompareTo(MetersPerSecond other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((MetersPerSecond)obj);
        }

        public override bool Equals(object obj)
        {
            if (obj is MetersPerSecond metersPerSecond)
            {
                return this == metersPerSecond;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();

        }

        public override string ToString()
        {
            return $"{Value}m/s";
        }
    }

    public static class MetersPerSecondExtensions
    {
        public static MetersPerSecond ToMetersPerSecond(this int value) => new MetersPerSecond(value);

        public static MetersPerSecond ToKmPerHour(this int value) => new MetersPerSecond((int)(value / 3.6));
    }
}