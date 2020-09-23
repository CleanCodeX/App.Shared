using System;

namespace Common.Shared.Attributes
{
    /// <summary>Adds an unit right to the value for generic displayed property</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DisplayUnitAttribute : Attribute
    {
        public string DisplayUnit { get; }

        public DisplayUnitAttribute(string unit) => DisplayUnit = unit;
    }
}