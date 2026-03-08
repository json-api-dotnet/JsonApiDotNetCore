using System.Security.Cryptography;
using System.Text;

namespace JsonApiDotNetCore.Serialization.Response;

/// <inheritdoc cref="IFingerprintGenerator" />
internal sealed class FingerprintGenerator : IFingerprintGenerator
{
    private static readonly byte[] Separator = "|"u8.ToArray();

    /// <inheritdoc />
    public string Generate(IEnumerable<string> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);

        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.MD5);

        foreach (string element in elements)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(element);
            hasher.AppendData(buffer);
            hasher.AppendData(Separator);
        }

        byte[] hash = hasher.GetHashAndReset();
        return Convert.ToHexString(hash);
    }
}
