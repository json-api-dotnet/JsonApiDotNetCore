namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization
{
    public interface IEncryptionService
    {
        string Encrypt(string value);

        string Decrypt(string value);
    }
}
