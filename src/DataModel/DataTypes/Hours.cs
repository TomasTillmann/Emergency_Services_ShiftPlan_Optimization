using System;

namespace ESSP.DataModel
{
    public readonly struct Hours : IComparable<Hours>, IComparable
    {
        public int Value { get; }

        public Hours(int value)
        {
            Value = value;
        }

        public static Hours MinValue => 0.ToHours();
        public static Hours MaxValue => (int.MaxValue / (2 * 60)).ToHours();

        #region Operators

        public static Hours operator +(Hours a, Hours b) => new Hours(a.Value + b.Value);
        public static Hours operator ++(Hours a) => new Hours(a.Value + 1); 
        public static Hours operator --(Hours a) => new Hours(a.Value - 1); 
        public static Hours operator -(Hours a, Hours b) => new Hours(a.Value - b.Value);
        public static bool operator <(Hours a, Hours b) => a.Value < b.Value;
        public static bool operator >(Hours a, Hours b) => a.Value > b.Value;
        public static bool operator <=(Hours a, Hours b) => a.Value <= b.Value;
        public static bool operator >=(Hours a, Hours b) => a.Value >= b.Value;
        public static bool operator ==(Hours a, Hours b) => a.Value == b.Value;
        public static bool operator !=(Hours a, Hours b) => a.Value != b.Value;

        #endregion

        public int CompareTo(Hours other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((Hours)obj);
        }

        public override bool Equals(object obj)
        {
            if (obj is Hours hour)
            {
                return this == hour;
            }

            return false;
        }
        public Minutes ToMinutes()
        {
            return new Minutes(Value * 60);
        }

        public Seconds ToSeconds()
        {
            return new Seconds(Value * 60 * 60);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Value}h";
        }


    }

    public static class HoursExtension
    {
        public static Hours ToHours(this int value) => new Hours(value);
    }
}