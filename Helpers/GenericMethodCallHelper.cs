using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Common.Shared.Extensions;

namespace Common.Shared.Helpers
{
    /*
     * Important Notes:
     * 1) A private instance or static methods located in any base class cannot be found. Only private member of same class are accessible by reflection. (there is a way though, but it requires the need to emit IL code) Remember this when creating derived test classes. To solve this, change the method access modifier to something different than private. 
     * 2) A non private static method located in any base class cannot be found if BindingFlags.FlattenHierarchy has not been specified. To solve this, either specify BindingFlags.FlattenHierarchy or pass the matching type of the method containing baseclass
     */
    public static class GenericMethodCallHelper
    {
        public static T CallGenericMethod<T>(object source, Type[] types, string methodName, BindingFlags bindingFlags, params object?[] parameters)
        {
            var method = GetMethodInfo(source.GetType(), methodName, bindingFlags);
            var genericMethod = method.MakeGenericMethod(types);

            return (T)genericMethod.Invoke(source, parameters)!;
        }

        public static T CallGenericMethod<T>(object source, Type t1, Type t2, string methodName, BindingFlags bindingFlags, params object?[] parameters)
        {
            var method = GetMethodInfo(source.GetType(), methodName, bindingFlags);
            var genericMethod = method.MakeGenericMethod(t1, t2);

            return (T)genericMethod.Invoke(source, parameters)!;
        }

        public static T CallGenericMethod<T>([NotNull] object source, [NotNull]  Type t1, [NotNull] string methodName, BindingFlags bindingFlags, params object?[] parameters)
        {
            var method = GetMethodInfo(source.GetType(), methodName, bindingFlags);
            var genericMethod = method.MakeGenericMethod(t1);

            return (T)Convert.ChangeType(genericMethod.Invoke(source, parameters), typeof(T))!;
        }

        public static T CallGenericMethod<T>(Type t1, string methodName, BindingFlags bindingFlags, params object?[] parameters)
        {
            var sourceType = typeof(T);
            var method = GetMethodInfo(sourceType, methodName, bindingFlags);
            var genericMethod = method.MakeGenericMethod(t1);

            return (T)genericMethod.Invoke(sourceType, parameters)!;
        }

        public static T CallGenericMethod<T>([NotNull] Type source, [NotNull]  Type t1, [NotNull] string methodName, BindingFlags bindingFlags, params object?[] parameters)
        {
            var method = GetMethodInfo(source, methodName, bindingFlags);
            var genericMethod = method.MakeGenericMethod(t1);

            return (T)Convert.ChangeType(genericMethod.Invoke(source, parameters), typeof(T))!;
        }

        public static T CallGenericMethod<T>(Type source, Type t1, string methodName, BindingFlags bindingFlags, Type[] paramTypes, params object?[] parameters)
        {
            var method = GetGenericMethodInfo(source, methodName, bindingFlags, paramTypes);
            var genericMethod = method.MakeGenericMethod(t1);

            return (T)genericMethod.Invoke(source, parameters)!;
        }

        public static T CallGenericMethod<T>(Type source, Type t1, Type t2, string methodName, BindingFlags bindingFlags, Type[] paramTypes, params object?[] parameters)
        {
            var method = GetGenericMethodInfo(source, methodName, bindingFlags, paramTypes);
            var genericMethod = method.MakeGenericMethod(t1, t2);

            return (T)genericMethod.Invoke(source, parameters)!;
        }

        public static T CallGenericMethod<T>(object source, Type t1, string methodName, BindingFlags bindingFlags, Type[] paramTypes, params object?[] parameters)
        {
            var method = GetGenericMethodInfo(source.GetType(), methodName, bindingFlags, paramTypes);
            var genericMethod = method.MakeGenericMethod(t1);

            return (T)genericMethod.Invoke(source, parameters)!;
        }

        private static MethodInfo GetMethodInfo(IReflect source, string methodName, BindingFlags bindingFlags)
        {
            Requires.NotDefault(bindingFlags, nameof(bindingFlags));

            var method = source.GetMethod(methodName, bindingFlags);
            Requires.NotNull(method, nameof(method));

            return method!;
        }

        private static MethodInfo GetGenericMethodInfo(IReflect source, string methodName, BindingFlags bindingFlags, Type[] types)
        {
            Requires.NotDefault(bindingFlags, nameof(bindingFlags));

            var method = source.GetMethod(methodName, bindingFlags, new GenericMethodBinder(), types, null);
            Requires.NotNull(method, nameof(method));

            return method!;
        }
    }

    public class GenericMethodBinder : Binder
    {
        private class BinderState
        {
            public object[]? Args;
        }

        public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo? culture) => default!;

        public override MethodBase BindToMethod(
            BindingFlags bindingAttr,
            MethodBase[] match,
            ref object?[] args,
            ParameterModifier[]? modifiers,
            CultureInfo? culture,
            string[]? names,
            out object state
            )
        {
            // Store the arguments to the method in a state object.
            var myBinderState = new BinderState();
            var arguments = new object[args.Length];
            args.CopyTo(arguments, 0);
            myBinderState.Args = arguments;
            state = myBinderState;
            if (match is null)
                throw new ArgumentNullException();

            // Find a method that has the same parameters as those of the args parameter.
            foreach (var method in match.Where(m => m.IsGenericMethod))
            {
                // Count the number of parameters that match.
                var count = 0;
                var parameters = method.GetParameters();
                // Go on to the next method if the number of parameters do not match.
                if (args.Length != parameters.Length)
                    continue;
                // Match each of the parameters that the user expects the method to have.
                for (var j = 0; j < args.Length; j++)
                {
                    // If the names parameter is not null, then reorder args.
                    if (names is not null)
                    {
                        Requires.Equal(names.Length, args.Length, "args.Length");
                        for (var k = 0; k < names.Length; k++)
                            if (string.CompareOrdinal(parameters[j].Name, names[k]) == 0)
                                args[j] = myBinderState.Args[k];
                    }
                    // Determine whether the types specified by the user can be converted to the parameter type.
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (ChangeType(args[j]!, parameters[j].ParameterType, culture) is not null)
                        count += 1;
                    else
                        break;
                }
                // Determine whether the method has been found.
                if (count == args.Length)
                    return method;
            }

            return null!;
        }

        public override object ChangeType(object value, Type type, CultureInfo? culture) => null!;

        public override void ReorderArgumentArray(ref object?[] args, object state)
        { }

        public override MethodBase? SelectMethod(
            BindingFlags bindingAttr,
            MethodBase[] match,
            Type[] types,
            ParameterModifier[]? modifiers
            )
        {
            match.ThrowIfNull(nameof(match));

            foreach (var method in match.Where(m => m.IsGenericMethod))
            {
                // Count the number of parameters that match.
                var count = 0;
                var parameters = method.GetParameters();
                // Go on to the next method if the number of parameters do not match.
                if (types.Length != parameters.Length)
                    continue;
                // Match each of the parameters that the user expects the method to have.
                for (var j = 0; j < types.Length; j++)
                    // Determine whether the types specified by the user can be converted to parameter type.
                    count += 1;

                // Determine whether the method has been found.
                if (count == types.Length)
                    return method;
            }
            return null;
        }

        public override PropertyInfo? SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type? returnType, Type[]? indexes, ParameterModifier[]? modifiers) => null;
    }

    
    public static class GenericMethodCallHelperExtensions
    {
        public static T CallGenericNonPublicStatic<T>(this object source, Type type, string methodName, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.NonPublic | BindingFlags.Static, parameters);

        public static T CallGenericPublicStatic<T>(this object source, Type type, string methodName, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.Public | BindingFlags.Static, parameters);

        public static T CallGenericNonPublicInstance<T>(this object source, Type type, string methodName, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.NonPublic | BindingFlags.Instance, parameters);

        public static T CallGenericPublicInstance<T>(this object source, Type type, string methodName, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.Public | BindingFlags.Instance, parameters);

        public static T CallGenericNonPublicStatic<T>(this object source, Type type, string methodName, Type[] paramTypes, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.NonPublic | BindingFlags.Static, paramTypes, parameters);

        public static T CallGenericPublicStatic<T>(this object source, Type type, string methodName, Type[] paramTypes, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.Public | BindingFlags.Static, paramTypes, parameters);

        public static T CallGenericNonPublicInstance<T>(this object source, Type type, string methodName, Type[] paramTypes, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.NonPublic | BindingFlags.Instance, paramTypes, parameters);

        public static T CallGenericPublicInstance<T>(this object source, Type type, string methodName, Type[] paramTypes, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.Public | BindingFlags.Instance, paramTypes, parameters);

        public static T CallGenericNonPublicStatic<T>(this Type source, Type type, string methodName, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.NonPublic | BindingFlags.Static, parameters);

        public static T CallGenericPublicStatic<T>(this Type source, Type type, string methodName, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.Public | BindingFlags.Static, parameters);

        public static T CallGenericNonPublicInstance<T>(this Type source, Type type, string methodName, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.NonPublic | BindingFlags.Instance, parameters);

        public static T CallGenericPublicInstance<T>(this Type source, Type type, string methodName, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.Public | BindingFlags.Instance, parameters);

        public static T CallGenericNonPublicStatic<T>(this Type source, Type type, string methodName, Type[] paramTypes, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.NonPublic | BindingFlags.Static, paramTypes, parameters);

        public static T CallGenericPublicStatic<T>(this Type source, Type type, string methodName, Type[] paramTypes, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.Public | BindingFlags.Static, paramTypes, parameters);

        public static T CallGenericNonPublicInstance<T>(this Type source, Type type, string methodName, Type[] paramTypes, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.NonPublic | BindingFlags.Instance, paramTypes, parameters);

        public static T CallGenericPublicInstance<T>(this Type source, Type type, string methodName, Type[] paramTypes, params object?[] parameters) =>
            GenericMethodCallHelper.CallGenericMethod<T>(source,
                type, methodName, BindingFlags.Public | BindingFlags.Instance, paramTypes, parameters);
    }
}