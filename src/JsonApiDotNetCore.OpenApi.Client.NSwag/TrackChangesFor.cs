using System.ComponentModel;

namespace JsonApiDotNetCore.OpenApi.Client.NSwag;

/// <summary>
/// Tracks assignment of property values, to support JSON:API partial POST/PATCH.
/// </summary>
/// <typeparam name="T">
/// The type whose property assignments to track.
/// </typeparam>
public sealed class TrackChangesFor<T>
    where T : INotifyPropertyChanged, new()
{
    public T Initializer { get; }

    public TrackChangesFor(JsonApiClient apiClient)
    {
        ArgumentNullException.ThrowIfNull(apiClient);

        Initializer = new T();
        apiClient.Track(Initializer);
    }
}
