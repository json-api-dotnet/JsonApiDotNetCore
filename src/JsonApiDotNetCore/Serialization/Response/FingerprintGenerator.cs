using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JsonApiDotNetCore.Serialization.Response
{
    /// <inheritdoc />
    internal sealed class FingerprintGenerator : IFingerprintGenerator
    {
        private static readonly byte[] Separator = Encoding.UTF8.GetBytes("|");
        private static readonly uint[] LookupTable = Enumerable.Range(0, 256).Select(ToLookupEntry).ToArray();

        private static uint ToLookupEntry(int index)
        {
            string hex = index.ToString("X2");
            return hex[0] + ((uint)hex[1] << 16);
        }

        /// <inheritdoc />
        public string Generate(IEnumerable<string> elements)
        {
            ArgumentGuard.NotNull(elements, nameof(elements));

            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.MD5);

            foreach (string element in elements)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(element);
                hasher.AppendData(buffer);
                hasher.AppendData(Separator);
            }

            byte[] hash = hasher.GetHashAndReset();
            return ByteArrayToHex(hash);
        }

        private static string ByteArrayToHex(byte[] bytes)
        {
            // https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa

            char[] buffer = new char[bytes.Length * 2];

            for (int index = 0; index < bytes.Length; index++)
            {
                uint value = LookupTable[bytes[index]];
                buffer[2 * index] = (char)value;
                buffer[2 * index + 1] = (char)(value >> 16);
            }

            return new string(buffer);
        }
    }
}
