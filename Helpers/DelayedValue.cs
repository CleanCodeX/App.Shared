using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Common.Shared.Extensions;

namespace Common.Shared.Helpers
{
    public class DelayedValue<T> : NotifyChangedValue<T>, IEquatable<DelayedValue<T>>
        where T : class?
    {
        // ReSharper disable once StaticMemberInGenericType
        public int DelayInMilliseconds = 1000;

        private CancellationTokenSource? _cts;

        // ReSharper disable once NotAccessedField.Local
        [SuppressMessage("Codequalität", "IDE0052:Ungelesene private Member entfernen")]
        private Task? _delayedSetTask;

#pragma warning disable CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".

        public DelayedValue() { }
        public DelayedValue(T value) : base(value) { }

        private T _valueToSet;
        public T ValueToSet
        {
            get => _valueToSet;
            set
            {
                if (OnValueChange is not null)
                    value = OnValueChange(value);

                _valueToSet = value;

                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                _delayedSetTask = Task.Delay(DelayInMilliseconds, _cts.Token)
                    .ContinueWith(t => ApplyValue(), _cts.Token);
            }
        }
#pragma warning restore CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".

        private void ApplyValue()
        {
            Interlocked.Exchange(ref _value, _valueToSet);

            OnPropertyChanged();
        }

        public override bool Equals(object? obj)
        {
            if (obj is null && _value is null) return true;

            return obj switch
            {
                T t => Equals(t),
                DelayedValue<T> dv => Equals(dv),
                _ => false
            };
        }

        public DelayedValue<T> Copy() => new DelayedValue<T>(_value!.Copy());

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(_value);

#pragma warning disable CS8638 // Der bedingte Zugriff generiert möglicherweise einen NULL-Wert für einen Typparameter.
        public bool Equals(DelayedValue<T>? obj) => Equals(obj?._value, _value);
        public override bool Equals(T? obj) => Equals(obj, _value);

        public static implicit operator DelayedValue<T>?(T source) => new DelayedValue<T> { Value = source };
        public static implicit operator T(DelayedValue<T>? source) => source!.Value;
    }
}