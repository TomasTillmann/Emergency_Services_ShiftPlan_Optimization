using System;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace ESSP.DataModel
{
    public struct Seconds : IComparable<Seconds>, IComparable
    {
        public int Value { get; }

        public Seconds(int value)
        {
            Value = value;
        }

        public static Seconds Duration(Seconds x, Seconds y)
        {
            return Math.Abs(x.Value - y.Value).ToSeconds();
        }

        public static Seconds MinValue => 0.ToSeconds();
        public static Seconds MaxValue => int.MaxValue.ToSeconds();

        #region Operators

        public static Seconds operator +(Seconds a, Seconds b) => new Seconds(a.Value + b.Value);
        public static Seconds operator -(Seconds a, Seconds b) => new Seconds(a.Value - b.Value);
        public static bool operator <(Seconds a, Seconds b) => a.Value < b.Value;
        public static bool operator >(Seconds a, Seconds b) => a.Value > b.Value;
        public static bool operator <=(Seconds a, Seconds b) => a.Value <= b.Value;
        public static bool operator >=(Seconds a, Seconds b) => a.Value >= b.Value;
        public static bool operator ==(Seconds a, Seconds b) => a.Value == b.Value;
        public static bool operator !=(Seconds a, Seconds b) => a.Value != b.Value;

        #endregion

        public int CompareTo(Seconds other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((Seconds)obj);
        }

        public override bool Equals(object obj)
        {
            if (obj is Seconds seconds)
            {
                return this == seconds;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Value}s";
        }
    }

    public static class SecondsExtensions
    {
        public static Seconds ToSeconds(this int value) => new Seconds(value);
    }
}