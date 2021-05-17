using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc />
    internal sealed class ETagGenerator : IETagGenerator
    {
        private static readonly uint[] LookupTable = Enumerable.Range(0, 256).Select(ToLookupEntry).ToArray();

        private static uint ToLookupEntry(int index)
        {
            string hex = index.ToString("X2");
            return hex[0] + ((uint)hex[1] << 16);
        }

        /// <inheritdoc />
        public EntityTagHeaderValue Generate(string requestUrl, string responseBody)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(requestUrl + "|" + responseBody);

            using HashAlgorithm hashAlgorithm = MD5.Create();
            byte[] hash = hashAlgorithm.ComputeHash(buffer);

            string eTagValue = "\"" + ByteArrayToHex(hash) + "\"";
            return EntityTagHeaderValue.Parse(eTagValue);
        }

        // https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
        private static string ByteArrayToHex(byte[] bytes)
        {
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
