using System.Globalization;
using System.Net;
using System.Text;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace OpenApiTests.IdObfuscation;

public sealed class HexadecimalCodec
{
    // This implementation is deliberately simple for demonstration purposes.
    // Consider using something more robust, such as https://github.com/sqids/sqids-dotnet.
    public static HexadecimalCodec Instance { get; } = new();

    private HexadecimalCodec()
    {
    }

    public long Decode(string? value)
    {
        if (value == null)
        {
            return 0;
        }

        if (!value.StartsWith('x'))
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
            {
                Title = "Invalid ID value.",
                Detail = $"The value '{value}' is not a valid hexadecimal value."
            });
        }

        string stringValue = FromHexString(value[1..]);
        return long.Parse(stringValue, CultureInfo.InvariantCulture);
    }

    private static string FromHexString(string hexString)
    {
        var bytes = new List<byte>(hexString.Length / 2);

        for (int index = 0; index < hexString.Length; index += 2)
        {
            string hexChar = hexString.Substring(index, 2);
            byte bt = byte.Parse(hexChar, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            bytes.Add(bt);
        }

        char[] chars = Encoding.ASCII.GetChars([.. bytes]);
        return new string(chars);
    }

    public string? Encode(long value)
    {
        if (value == 0)
        {
            return null;
        }

        string stringValue = value.ToString(CultureInfo.InvariantCulture);
        return $"x{ToHexString(stringValue)}";
    }

    private static string ToHexString(string value)
    {
        var builder = new StringBuilder();

        foreach (byte bt in Encoding.ASCII.GetBytes(value))
        {
            builder.Append(bt.ToString("X2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
