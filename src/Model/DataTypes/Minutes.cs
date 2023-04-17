using System;

namespace ESSP.DataModel
{
    public readonly struct Minutes : IComparable<Minutes>, IComparable
    {
        public int Value { get; }

        public Minutes(int value)
        {
            Value = value;
        }

        public static Minutes MinValue => 0.ToMinutes();
        public static Minutes MaxValue => (int.MaxValue / 60).ToMinutes();

        #region Operators

        public static Minutes operator +(Minutes a, Minutes b) => new Minutes(a.Value + b.Value);
        public static Minutes operator -(Minutes a, Minutes b) => new Minutes(a.Value - b.Value);
        public static bool operator <(Minutes a, Minutes b) => a.Value < b.Value;
        public static bool operator >(Minutes a, Minutes b) => a.Value > b.Value; 
        public static bool operator <=(Minutes a, Minutes b) => a.Value <= b.Value;
        public static bool operator >=(Minutes a, Minutes b) => a.Value >= b.Value; 
        public static bool operator ==(Minutes a, Minutes b) => a.Value == b.Value; 
        public static bool operator !=(Minutes a, Minutes b) => a.Value != b.Value;

        #endregion

        public int CompareTo(Minutes other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((Minutes)obj);
        }

        public override bool Equals(object obj)
        {
            return ((Minutes)obj).Value == ((Minutes)obj).Value; 
        }

        public Seconds ToSeconds()
        {
            return new Seconds(Value * 60);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode(); 
        }

        public override string ToString()
        {
            return $"{Value}min"; 
        }
    }

    public static class MinutesExtensions
    {
        public static Minutes ToMinutes(this int value) => new Minutes(value);
    }
}