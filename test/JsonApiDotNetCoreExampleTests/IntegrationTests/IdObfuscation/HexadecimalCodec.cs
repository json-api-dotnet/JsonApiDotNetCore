using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.IdObfuscation
{
    internal sealed class HexadecimalCodec
    {
        public int Decode(string value)
        {
            if (value == null)
            {
                return 0;
            }

            if (!value.StartsWith("x", StringComparison.Ordinal))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Invalid ID value.",
                    Detail = $"The value '{value}' is not a valid hexadecimal value."
                });
            }

            string stringValue = FromHexString(value.Substring(1));
            return int.Parse(stringValue);
        }

        private static string FromHexString(string hexString)
        {
            var bytes = new List<byte>(hexString.Length / 2);

            for (int index = 0; index < hexString.Length; index += 2)
            {
                string hexChar = hexString.Substring(index, 2);
                byte bt = byte.Parse(hexChar, NumberStyles.HexNumber);
                bytes.Add(bt);
            }

            char[] chars = Encoding.ASCII.GetChars(bytes.ToArray());
            return new string(chars);
        }

        public string Encode(int value)
        {
            if (value == 0)
            {
                return null;
            }

            string stringValue = value.ToString();
            return 'x' + ToHexString(stringValue);
        }

        private static string ToHexString(string value)
        {
            var builder = new StringBuilder();

            foreach (byte bt in Encoding.ASCII.GetBytes(value))
            {
                builder.Append(bt.ToString("X2"));
            }

            return builder.ToString();
        }
    }
}
