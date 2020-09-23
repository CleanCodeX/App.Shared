using System;
using System.Diagnostics;

namespace Common.Shared.Helpers
{
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct Timestamp : IComparable, IEquatable<Timestamp>, IComparable<Timestamp>
    {
#pragma warning disable CA1805 
        public static readonly Timestamp Zero = default;
#pragma warning restore CA1805 

        private Timestamp(ulong value) => Value = value;

        public ulong Value { get; }

        public static implicit operator Timestamp(ulong value) => new Timestamp(value);
        public static implicit operator Timestamp(long value) => new Timestamp(unchecked((ulong)value));
        public static explicit operator Timestamp(byte[] value)
        {
            return new Timestamp(((ulong)value[0] << 56) | ((ulong)value[1] << 48) | ((ulong)value[2] << 40) | ((ulong)value[3] << 32) | ((ulong)value[4] << 24) | ((ulong)value[5] << 16) | ((ulong)value[6] << 8) | value[7]);
        }
        public static explicit operator Timestamp?(byte[]? value)
        {
            if (value is null) return null;

            return new Timestamp(((ulong)value[0] << 56) | ((ulong)value[1] << 48) | ((ulong)value[2] << 40) | ((ulong)value[3] << 32) | ((ulong)value[4] << 24) | ((ulong)value[5] << 16) | ((ulong)value[6] << 8) | value[7]);
        }
        public static implicit operator byte[](Timestamp timestamp)
        {
            var r = new byte[8];

            r[0] = (byte)(timestamp.Value >> 56);
            r[1] = (byte)(timestamp.Value >> 48);
            r[2] = (byte)(timestamp.Value >> 40);
            r[3] = (byte)(timestamp.Value >> 32);
            r[4] = (byte)(timestamp.Value >> 24);
            r[5] = (byte)(timestamp.Value >> 16);
            r[6] = (byte)(timestamp.Value >> 8);
            r[7] = (byte)timestamp.Value;

            return r;
        }

        public override bool Equals(object? obj) => obj is Timestamp timestamp && Equals(timestamp);

        public override int GetHashCode() => Value.GetHashCode();

        public bool Equals(Timestamp other) => other.Value == Value;

        int IComparable.CompareTo(object? obj) => obj is null ? 1 : CompareTo((Timestamp)obj);

        public int CompareTo(Timestamp other) => Value == other.Value ? 0 : Value < other.Value ? -1 : 1;

        public static bool operator ==(Timestamp comparand1, Timestamp comparand2) => comparand1.Equals(comparand2);
        public static bool operator !=(Timestamp comparand1, Timestamp comparand2) => !comparand1.Equals(comparand2);

        public static bool operator >(Timestamp comparand1, Timestamp comparand2) => comparand1.CompareTo(comparand2) > 0;
        public static bool operator >=(Timestamp comparand1, Timestamp comparand2) => comparand1.CompareTo(comparand2) >= 0;

        public static bool operator <(Timestamp comparand1, Timestamp comparand2) => comparand1.CompareTo(comparand2) < 0;
        public static bool operator <=(Timestamp comparand1, Timestamp comparand2) => comparand1.CompareTo(comparand2) <= 0;

        public override string ToString() => Value.ToString("x16");

        public static Timestamp Max(Timestamp comparand1, Timestamp comparand2) => comparand1.Value < comparand2.Value ? comparand2 : comparand1;
    }
}
