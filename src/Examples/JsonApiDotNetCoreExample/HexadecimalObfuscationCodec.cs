using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JsonApiDotNetCoreExample
{
    public static class HexadecimalObfuscationCodec
    {
        public static int Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            if (!value.StartsWith("x"))
            {
                throw new InvalidOperationException("Invalid obfuscated id.");
            }

            string stringValue = FromHexString(value.Substring(1));
            return int.Parse(stringValue);
        }

        private static string FromHexString(string hexString)
        {
            List<byte> bytes = new List<byte>(hexString.Length / 2);
            for (int index = 0; index < hexString.Length; index += 2)
            {
                var hexChar = hexString.Substring(index, 2);
                byte bt = byte.Parse(hexChar, NumberStyles.HexNumber);
                bytes.Add(bt);
            }

            var chars = Encoding.ASCII.GetChars(bytes.ToArray());
            return new string(chars);
        }

        public static string Encode(object value)
        {
            if (value is int intValue && intValue == 0)
            {
                return string.Empty;
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
