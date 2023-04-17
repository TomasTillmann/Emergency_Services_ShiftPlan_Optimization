using System;

namespace ESSP.DataModel
{
    public readonly struct Meters : IComparable<Meters>
    {
        public int Value { get; }

        public Meters(int value)
        {
            Value = value;
        }

        public static Meters MinValue => 0.ToMeters();
        public static Meters MaxValue => int.MaxValue.ToMeters();

        public static Meters Distance(Meters x, Meters y)
        {
            return Math.Abs(x.Value - y.Value).ToMeters();
        }

        #region Operators

        public static Meters operator +(Meters a, Meters b) => new Meters(a.Value + b.Value);
        public static Meters operator -(Meters a, Meters b) => new Meters(a.Value - b.Value);
        public static bool operator <(Meters a, Meters b) => a.Value < b.Value;
        public static bool operator >(Meters a, Meters b) => a.Value > b.Value; 
        public static bool operator <=(Meters a, Meters b) => a.Value <= b.Value;
        public static bool operator >=(Meters a, Meters b) => a.Value >= b.Value; 
        public static bool operator ==(Meters a, Meters b) => a.Value == b.Value; 
        public static bool operator !=(Meters a, Meters b) => a.Value != b.Value;

        // v = s / t
        public static MetersPerSecond operator /(Meters a, Seconds b) => new MetersPerSecond(a.Value / b.Value);
        // t = s / v
        public static Seconds operator /(Meters a, MetersPerSecond b) => new Seconds(a.Value / b.Value);

        #endregion

        public int CompareTo(Meters other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((Meters)obj);
        }

        public override bool Equals(object obj)
        {
            return ((Meters)obj).Value == ((Meters)obj).Value; 
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Value}m"; 
        }
    }

    public static class MetersExtensions
    {
        public static Meters ToMeters(this int value) => new Meters(value);
    }
}