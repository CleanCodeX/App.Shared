using System.Reflection;
using System.Text;
using Common.Shared.Internal.Helpers;

namespace Common.Shared.Internal.Extensions
{
    internal static class StringBuilderExtensions
    {
        public static void AppendObjectInstanceProperties(this StringBuilder source, object instance) => source.AppendLine(FormatHelper.FormatObject(instance, inheritProperties: true));

        public static void AppendObjectProperties(this StringBuilder source, object instance, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            foreach (var memberInfo in instance.GetType().GetProperties(bindingFlags))
                source.AppendLine(
                    $"{memberInfo.Name}: {memberInfo.GetValue(instance).FormatValue(memberInfo)}");
        }

        public static void AppendObjectFields(this StringBuilder source, object instance, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            foreach (var memberInfo in instance.GetType().GetFields(bindingFlags))
                source.AppendLine(
                    $"{memberInfo.Name}: {memberInfo.GetValue(instance).FormatValue(memberInfo)}");
        }
    }
}
