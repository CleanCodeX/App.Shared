using System;

namespace Common.Shared.Attributes
{
    /// <summary>Indicates that this property should not be displayed automatically</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public sealed class HideAttribute : Attribute
    { }
}