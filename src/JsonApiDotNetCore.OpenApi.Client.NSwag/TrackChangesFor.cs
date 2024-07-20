using System.ComponentModel;

namespace JsonApiDotNetCore.OpenApi.Client.NSwag;

public sealed class TrackChangesFor<T>
    where T : INotifyPropertyChanged, new()
{
    public T Initializer { get; }

    public TrackChangesFor(NewJsonApiClient apiClient)
    {
        ArgumentNullException.ThrowIfNull(apiClient);

        Initializer = new T();
        apiClient.Track(Initializer);
    }
}
