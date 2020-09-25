using Common.Shared.Attributes;
using Common.Shared.Properties;

namespace Common.Shared.Internal.Attributes
{
    internal class InternalLocalizedDisplayNameAttribute : LocalizedDisplayNameAttribute
    {
        public InternalLocalizedDisplayNameAttribute(string displayNameKey) : base(displayNameKey, typeof(Resources))
        { }
    }
}