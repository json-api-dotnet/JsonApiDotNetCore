using System.Text;

namespace JsonApiDotNetCore.Extensions
{
    public static class StringExtensions
    {
        public static string Dasherize(this string str)
        {
            var chars = str.ToCharArray();
            if (chars.Length > 0)
            {
                var builder = new StringBuilder();
                for (var i = 0; i < chars.Length; i++)
                {
                    if (char.IsUpper(chars[i]))
                    {
                        var hashedString = (i > 0) ? $"-{char.ToLower(chars[i])}" : $"{char.ToLower(chars[i])}";
                        builder.Append(hashedString);
                    }
                    else
                    {
                        builder.Append(chars[i]);
                    }
                }
                return builder.ToString();
            }
            return str;
        }
    }
}
