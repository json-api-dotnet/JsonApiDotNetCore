using System.Security.Cryptography;
using System.Text;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

public sealed class AesEncryptionService : IEncryptionService
{
    private static readonly byte[] CryptoKey = Encoding.UTF8.GetBytes("Secret!".PadRight(32, '-'));

    public string Encrypt(string value)
    {
        using SymmetricAlgorithm cipher = CreateCipher();

        using ICryptoTransform transform = cipher.CreateEncryptor();
        byte[] plaintext = Encoding.UTF8.GetBytes(value);
        byte[] cipherText = transform.TransformFinalBlock(plaintext, 0, plaintext.Length);

        byte[] buffer = new byte[cipher.IV.Length + cipherText.Length];
        Buffer.BlockCopy(cipher.IV, 0, buffer, 0, cipher.IV.Length);
        Buffer.BlockCopy(cipherText, 0, buffer, cipher.IV.Length, cipherText.Length);

        return Convert.ToBase64String(buffer);
    }

    public string Decrypt(string value)
    {
        byte[] buffer = Convert.FromBase64String(value);

        using SymmetricAlgorithm cipher = CreateCipher();

        byte[] initVector = new byte[cipher.IV.Length];
        Buffer.BlockCopy(buffer, 0, initVector, 0, initVector.Length);
        cipher.IV = initVector;

        using ICryptoTransform transform = cipher.CreateDecryptor();
        byte[] plainBytes = transform.TransformFinalBlock(buffer, initVector.Length, buffer.Length - initVector.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private static SymmetricAlgorithm CreateCipher()
    {
        var cipher = Aes.Create();
        cipher.Key = CryptoKey;

        return cipher;
    }
}
