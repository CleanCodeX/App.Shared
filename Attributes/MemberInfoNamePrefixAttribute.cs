using System;

namespace Common.Shared.Attributes
{
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public sealed class MemberInfoNamePrefixAttribute : Attribute
    {
        public string NamePrefix { get; }

        public MemberInfoNamePrefixAttribute(string prefix) => NamePrefix = prefix;
    }
}