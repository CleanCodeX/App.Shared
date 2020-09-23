using System;

namespace Common.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class FormatAsTimespanSinceNowAttribute : Attribute
    { }
}