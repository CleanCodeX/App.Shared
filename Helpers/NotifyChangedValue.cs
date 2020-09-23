using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Common.Shared.Extensions;
using Res = Common.Shared.Properties.Resources;

// ReSharper disable InconsistentNaming

namespace Common.Shared.Helpers
{
    public interface INotifyChangedValue : INotifyPropertyChanged
    {
        void Reset();
        bool IsModified { get; }
        object? Value { get; set; }
        string Name { get; }
    }

    [DebuggerDisplay("{(Name ?? typeof(T).Name),nq} | Value: {ToString()} | Modified: {IsModified}")]
    public class NotifyChangedValue<T> : IEquatable<NotifyChangedValue<T>>, IEquatable<T>, INotifyChangedValue
    {
        private readonly Func<T>? _valueFactory;

        public event PropertyChangedEventHandler? PropertyChanged;

#pragma warning disable CS8766
        public string? Name { get; set; }
#pragma warning restore CS8766

        public bool IsModified => !(Equals(_value, typeof(T).GetDefaultOfType()) || _value!.DeepCompareAreEqual(typeof(T).GetDefaultOfType()!));

        protected T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;

                if (OnValueChange is not null)
                    value = OnValueChange(value);
                
                _value = value;

                OnPropertyChanged();
            }
        }

        public Func<T, T>? OnValueChange { get; set; }

        object? INotifyChangedValue.Value
        {
            get => _value;
            set => Value = (T)value!;
        }

        public Func<T, string?>? ToStringFactory { get; set; }

        public override string? ToString()
        {
            if (ToStringFactory is not null)
                return ToStringFactory(Value);

            var debugAttr = typeof(T).GetCustomAttribute<DebuggerDisplayAttribute>();
            if (debugAttr is not null)
                return debugAttr.ToString();

            if (_value is ICollection col)
                return $"{Res.Count}: {col.Count}";

            return _value?.ToString();
        }

#pragma warning disable CS8618
        public NotifyChangedValue(Func<T> valueFactory, string name) : this(valueFactory) => Name = name;
        public NotifyChangedValue(Func<T> valueFactory)
        {
            _valueFactory = valueFactory;
            _value = valueFactory();
        }
        public NotifyChangedValue(T value) => _value = value;
        public NotifyChangedValue(T value, string name) : this(name) => _value = value;
        public NotifyChangedValue(string name) => Name = name;
        public NotifyChangedValue() => _value = default!;
#pragma warning restore CS8618 

        public void Reset() => Value = (_valueFactory is not null ? _valueFactory() : default)!;

        protected void OnPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name ?? Res.Value));

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;

            return obj switch
            {
                NotifyChangedValue<T> ncValue => Equals(ncValue),
                T value => Equals(value),
                _ => false
            };
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(_value!);

#pragma warning disable CS8638 // Der bedingte Zugriff generiert möglicherweise einen NULL-Wert für einen Typparameter.
        public bool Equals(NotifyChangedValue<T>? obj) => obj is not null && Equals(obj.Value, Value);
        public virtual bool Equals(T? obj) => Equals(obj, Value);

        public static implicit operator NotifyChangedValue<T>?(T source) => new NotifyChangedValue<T>(source);
        public static implicit operator T(NotifyChangedValue<T>? source) => source!.Value;
    }
}