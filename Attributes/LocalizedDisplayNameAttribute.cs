using System;
using System.ComponentModel;
using System.Reflection;
using Common.Shared.Extensions;

namespace Common.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Enum)]
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayNameAttribute(string displayNameKey, Type resourceType)
            : base(displayNameKey)
        {
            resourceType.ThrowIfNull(nameof(resourceType));

            var nameProperty = resourceType!.GetProperty(base.DisplayName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (nameProperty is null)
                return;

            DisplayNameValue = (string)nameProperty.GetValue(nameProperty.DeclaringType, null)!;
        }
    }
}