using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Common.Shared.Extensions;

namespace Common.Shared.Helpers
{
    public interface IFormattedItems
    {
        IEnumerable<FormattedItem> GetFormattedItems();
    }

    [DebuggerDisplay("{" + nameof(Name) + ",nq}: {" + nameof(Data) + ",nq}")]
    public class FormattedItem
    {
        public enum VerboseVisibility
        {
            Always = 0,
            VerboseOnly = 1,
            NotVerboseOnly = 2
        }

        public VerboseVisibility VerboseMode { get; }
        public string Data { get; }
        public string Name { get; }
        public string? DisplayName { get; }
        public bool AddName { get; }

        public FormattedItem(string name, string data, string displayName) : this(name, data) => (DisplayName, AddName) = (displayName, true);
        public FormattedItem(string name, string data, bool addName) : this(name, data) => AddName = addName;
        public FormattedItem(string name, string data, VerboseVisibility verboseMode = VerboseVisibility.Always) => (Name, Data, VerboseMode) = (name, data, verboseMode);
        public FormattedItem(string name, string data, VerboseVisibility verboseMode, bool addName) : this(name, data, verboseMode) => AddName = addName;

        public static string AsString(IFormattedItems instance, bool verbose = false) => AsString(instance.GetFormattedItems(), verbose);
        public static string AsString(IEnumerable<FormattedItem> items, bool verbose = false)
        {
            var sb = new StringBuilder();
            var lastWasLinebreak = false;
            var firstEntry = true;
            const string valueTemplate = " | {0}";

            foreach (var item in items)
                switch (item.VerboseMode)
                {
                    case VerboseVisibility.VerboseOnly when !verbose:
                    case VerboseVisibility.NotVerboseOnly when verbose:
                        continue;
                    default:
                        string valuetoAdd;
                        var value = (item.AddName ? $"{item.DisplayName ?? item.Name}: " : string.Empty) + item.Data;

                        if (firstEntry || lastWasLinebreak || value == Environment.NewLine)
                            valuetoAdd = value;
                        else
                            valuetoAdd = valueTemplate.InsertArgs(value);

                        lastWasLinebreak = value == Environment.NewLine;

                        sb.Append(valuetoAdd);

                        firstEntry = false;

                        break;
                }

            return sb.ToString();
        }
    }
}
