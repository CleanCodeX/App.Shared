using Common.Shared.Attributes;
using KellermanSoftware.CompareNetObjects;

// ReSharper disable ClassNeverInstantiated.Global

namespace Common.Shared.Helpers
{
    public static class CompareHelper
    {
        public static readonly ComparisonConfig CompareConfig = new ComparisonConfig
        {
            AttributesToIgnore = { typeof(DoNotCompareAttribute) },
            CompareStaticFields = false,
            CompareStaticProperties = false,
            CompareFields = false
        };

        private static readonly CompareLogic CompareLogic = new CompareLogic(CompareConfig);

        public static ComparisonResult Compare(object expectedObject, object actualObject) => CompareLogic.Compare(expectedObject, actualObject);
    }
}