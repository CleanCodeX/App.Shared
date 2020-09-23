using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Common.Shared.Attributes;
using Common.Shared.Extensions;
using Common.Shared.Extensions.Enumerables;

namespace Common.Shared.Helpers
{
    public static class PropertyCopier
    {
        /// <summary>
        /// Copies values from one object to another. Source or target properties decorated with <see cref="DoNotUpdateAttribute"/> won't be copied.
        /// Source properties need to be public and readable, target properties to be public and read writable.
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="deepCopy">If set to true, subobjects will be copied, too.</param>
        public static void CopyPropertyValuesTo(object source, object target, bool deepCopy = true)
            => CopyPropertyValuesTo(source.GetType(), source, target, deepCopy);

        /// <summary>
        /// Copies values from one object to another. Source or target properties decorated with <see cref="DoNotUpdateAttribute"/> won't be copied.
        /// Source properties need to be public and readable, target properties to be public and read writable.
        /// </summary>
        /// <typeparam name="TContract">Contract type of properties to be copied.</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="deepCopy">If set to true, subobjects will be copied, too.</param>
        public static void CopyPropertyValuesTo<TContract>(object source, object target, bool deepCopy = true)
            => CopyPropertyValuesTo(typeof(TContract), source, target, deepCopy);

        /// <summary>
        /// Copies values from one object to another. Source or target properties decorated with <see cref="DoNotUpdateAttribute"/> won't be copied.
        /// Source properties need to be public and readable, target properties to be public and read writable.
        /// </summary>
        /// <param name="contractType">Contract type of source object</param>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="deepCopy">If set to true, subobjects will be copied, too.</param>
        public static void CopyPropertyValuesTo(Type contractType, object source, object target, bool deepCopy = true)
        {
            var sourceProps = contractType.GetPublicProperties().Where(p => p.CanRead && !p.IsDefined<DoNotUpdateAttribute>()).ToList();
            var targetProps = target.GetType().GetPublicProperties().Where(p => p.CanRead && p.CanWrite && !p.IsDefined<DoNotUpdateAttribute>()).ToList();

            SuppressPropertyChangedNotifications(true);

            try
            {
                var targetPropNames = targetProps.Select(p => p.Name).ToHashSet();
                var matchingProps = sourceProps.Where(p => targetPropNames.Contains(p.Name));
                foreach (var sourceProp in matchingProps)
                {
                    var targetProp = targetProps.First(x => x.Name == sourceProp.Name);
                    var targetPropType = targetProp.PropertyType;
                    var sourceValue = sourceProp.GetValue(source, null);
                    var sourcePropType = sourceProp.PropertyType;

                    if ((sourcePropType != typeof(string) || sourcePropType.IsArray) && sourcePropType.Implements<IEnumerable>())
                    {
                        sourceValue ??= Array.Empty<object>();

                        if (sourceValue is not IList sourceCollection)
                            sourceCollection = ((IEnumerable<object>)sourceValue).ToArray();

                        Type? elementType = null;
                        IList targetCollection;
                        if (sourcePropType.BaseType == typeof(Array))
                            // ReSharper disable once AssignNullToNotNullAttribute
                            targetCollection = Array.CreateInstance(sourcePropType.GetElementType()!, sourceCollection.Count);
                        else if (sourcePropType.BaseType is null || sourcePropType.IsInterface)
                        {
                            var genericArgs = targetPropType.GetGenericArguments();

                            var constructedListType = typeof(ICollection<>).MakeGenericType(genericArgs);
                            if (constructedListType.IsAssignableFrom(targetPropType))
                            {
                                constructedListType = typeof(Collection<>).MakeGenericType(genericArgs);
                                TryCollectionOfWhat(targetPropType, out elementType);
                            }
                            else
                            {
                                constructedListType = typeof(List<>).MakeGenericType(genericArgs);
                                TryListOfWhat(targetPropType, out elementType);
                            }

                            targetCollection = (IList)Activator.CreateInstance(constructedListType)!;
                        }
                        else
                            targetCollection = (IList)Activator.CreateInstance(sourcePropType, null)!;

                        // Map properties
                        foreach (var item in sourceCollection.ToNotNull())
                        {
                            elementType ??= item.GetType();

                            // New item instance
                            var newItem = HasDefaultConstructor(elementType)
                                ? Activator.CreateInstance(elementType, null)!
                                : item;

                            // Map properties
                            CopyPropertyValuesTo(item, newItem, deepCopy);

                            // Add to destination collection
                            if (sourcePropType.BaseType == typeof(Array))
                            {
                                if (targetCollection is not null) targetCollection[sourceCollection.IndexOf(item)] = newItem;
                            }
                            else if (targetCollection is { } list)
                                list.Add(newItem);
                        }

                        // Add new collection to destination
                        targetProp.SetValue(target, targetCollection, null);
                    }
                    else if ((sourcePropType.IsClass || sourcePropType.IsInterface) && sourcePropType != typeof(string))
                    {
                        var sourcePropCreated = false;
                        if (sourcePropType.IsClass && !sourcePropType.IsAbstract && sourceValue is null &&
                            HasDefaultConstructor(sourcePropType))
                        {
                            sourceValue = Activator.CreateInstance(sourceProp.PropertyType);
                            sourcePropCreated = true;
                        }

                        if (sourceValue is null) continue;
                        if (!deepCopy) continue;

                        var targetValue = targetProp.GetValue(target, null);

                        switch (targetValue)
                        {
                            case null when sourcePropCreated:
                                continue;
                            case null when targetPropType.IsClass && !targetPropType.IsAbstract && HasDefaultConstructor(targetPropType):
                                targetValue = Activator.CreateInstance(targetPropType);
                                targetProp.SetValue(target, targetValue, null);
                                break;
                        }

                        if (targetValue is null) continue;

                        // ReSharper disable once RedundantArgumentDefaultValue
                        CopyPropertyValuesTo(sourceValue, targetValue, true);
                    }
                    else if (targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                    {
                        var targetValue = targetProp.GetValue(target, null);
                        if (!Equals(sourceValue, targetValue))
                            targetProp.SetValue(target, sourceValue, null);
                    }
                }
            }
            finally
            {
                SuppressPropertyChangedNotifications(false);
            }

            void SuppressPropertyChangedNotifications(bool suppressEvents)
            {
                if (target is INotifyPropertyChangedObject notifyPropertyChangedObject)
                    notifyPropertyChangedObject.SuppressEvents = suppressEvents;
            }
        }

        /// <summary>
        /// Determines whether the type has a default contructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static bool HasDefaultConstructor(Type type)
        {
            return
                type.GetConstructor(Type.EmptyTypes) is not null ||
                type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Any(x => x.GetParameters().All(p => p.IsOptional));
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool TryListOfWhat(Type type, out Type? innerType)
        {
            var interfaceTest = new Func<Type, Type?>(i =>
                (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)
                    ? i.GetGenericArguments().Single()
                    : null)
            );

            innerType = interfaceTest(type);
            if (innerType is not null)
                return true;

            foreach (var i in type.GetInterfaces())
            {
                innerType = interfaceTest(i);
                if (innerType is not null)
                    return true;
            }

            return false;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool TryCollectionOfWhat(Type type, out Type? innerType)
        {
            var interfaceTest = new Func<Type, Type?>(i =>
                (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)
                    ? i.GetGenericArguments().Single()
                    : null)!
            );

            innerType = interfaceTest(type);
            if (innerType is not null)
                return true;

            foreach (var i in type.GetInterfaces())
            {
                innerType = interfaceTest(i);
                if (innerType is not null)
                    return true;
            }

            return false;
        }
    }
}