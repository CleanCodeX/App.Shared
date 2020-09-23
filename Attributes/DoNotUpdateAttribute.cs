using System;

namespace Common.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public sealed class DoNotUpdateAttribute : Attribute
    { }
}
