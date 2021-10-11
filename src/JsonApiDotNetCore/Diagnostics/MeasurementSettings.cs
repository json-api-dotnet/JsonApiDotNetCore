#nullable disable

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.Diagnostics
{
    internal static class MeasurementSettings
    {
        public static readonly bool ExcludeDatabaseInPercentages = bool.Parse(bool.TrueString);
        public static readonly bool ExcludeJsonSerializationInPercentages = bool.Parse(bool.FalseString);
    }
}
