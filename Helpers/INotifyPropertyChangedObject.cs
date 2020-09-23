using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Common.Shared.Helpers
{
    public static class NotifyPropertyChangedObjectExtensions
    {
        public static void MarkAsUnmodified([NotNull] this INotifyPropertyChangedObject source)
        {
            if (!source.IsModified) return;

            source.IsModified = false;
            source.OnPropertyChanged(nameof(source.IsModified)); 
        }
    }

    public interface IIsModifiedViewModel
    {
        bool IsModified { get; set; }
    }

    public interface ISuppressEventsViewModel
    {
        bool SuppressEvents { get; set; }
    }

    public interface INotifyPropertyChangedObject : INotifyPropertyChanged, IIsModifiedViewModel, ISuppressEventsViewModel
    {
        void OnPropertyChanged(string propertyName);
    }
}