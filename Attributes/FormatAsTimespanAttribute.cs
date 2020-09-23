using System;
using Common.Shared.Enums;

namespace Common.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class FormatAsTimespanAttribute : Attribute
    {
        public TimeUnit Unit { get; }

        public FormatAsTimespanAttribute(TimeUnit unit) => Unit = unit;
    }
}