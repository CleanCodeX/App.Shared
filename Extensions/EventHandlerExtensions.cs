using System;

namespace Common.Shared.Extensions
{
    public static class EventHandlerExtensions
    {
        public static void Raise<T>(this EventHandler<T>? @event, object sender, T args) where T : EventArgs =>
            @event?.Invoke(sender, args);
    }
}
