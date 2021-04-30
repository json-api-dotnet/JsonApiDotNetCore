namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Serialization
{
    public sealed class SerializationHitCounter
    {
        internal int DeserializeCount { get; private set; }
        internal int SerializeCount { get; private set; }

        internal void Reset()
        {
            DeserializeCount = 0;
            SerializeCount = 0;
        }

        internal void IncrementDeserializeCount()
        {
            DeserializeCount++;
        }

        internal void IncrementSerializeCount()
        {
            SerializeCount++;
        }
    }
}
