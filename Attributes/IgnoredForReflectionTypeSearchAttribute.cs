using System;

namespace Common.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class IgnoredForReflectionTypeSearchAttribute : Attribute
    {
    }
}