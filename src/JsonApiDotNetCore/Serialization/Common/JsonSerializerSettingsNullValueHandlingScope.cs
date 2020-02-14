using System;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Common
{
    /// <summary>
    /// Used to temporarily switch to a different <see cref="JsonSerializerSettings.NullValueHandling"/> value while preserving other settings.
    /// After disposal, the original NullValueHandling value is restored.
    /// </summary>
    internal sealed class JsonSerializerSettingsNullValueHandlingScope : IDisposable
    {
        private readonly NullValueHandling _beforeNullValueHandling;

        public JsonSerializerSettings Settings { get; set; }

        public JsonSerializerSettingsNullValueHandlingScope(JsonSerializerSettings settings,
            NullValueHandling nullValueHandling)
        {
            _beforeNullValueHandling = settings.NullValueHandling;
            Settings = settings;

            Settings.NullValueHandling = nullValueHandling;
        }

        public void Dispose()
        {
            Settings.NullValueHandling = _beforeNullValueHandling;
        }
    }
}
