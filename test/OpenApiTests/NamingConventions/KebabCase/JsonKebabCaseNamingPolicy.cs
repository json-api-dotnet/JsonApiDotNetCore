using System.Text;
using System.Text.Json;

namespace OpenApiTests.NamingConventions.KebabCase;

// Based on https://github.com/J0rgeSerran0/JsonNamingPolicy
internal sealed class JsonKebabCaseNamingPolicy : JsonNamingPolicy
{
    private const char Separator = '-';

    public static readonly JsonKebabCaseNamingPolicy Instance = new();

    public override string ConvertName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        ReadOnlySpan<char> spanName = name.Trim();

        var stringBuilder = new StringBuilder();
        bool addCharacter = true;

        bool isNextLower = false;
        bool isNextUpper = false;
        bool isNextSpace = false;

        for (int position = 0; position < spanName.Length; position++)
        {
            if (position != 0)
            {
                bool isCurrentSpace = spanName[position] == 32;
                bool isPreviousSpace = spanName[position - 1] == 32;
                bool isPreviousSeparator = spanName[position - 1] == 95;

                if (position + 1 != spanName.Length)
                {
                    isNextLower = spanName[position + 1] is >= 'a' and <= 'z';
                    isNextUpper = spanName[position + 1] is >= 'A' and <= 'Z';
                    isNextSpace = spanName[position + 1] == ' ';
                }

                if (isCurrentSpace && (isPreviousSpace || isPreviousSeparator || isNextUpper || isNextSpace))
                {
                    addCharacter = false;
                }
                else
                {
                    bool isCurrentUpper = spanName[position] is >= 'A' and <= 'Z';
                    bool isPreviousLower = spanName[position - 1] is >= 'a' and <= 'z';
                    bool isPreviousNumber = spanName[position - 1] is >= '0' and <= '9';

                    if (isCurrentUpper && (isPreviousLower || isPreviousNumber || isNextLower || isNextSpace))
                    {
                        stringBuilder.Append(Separator);
                    }
                    else
                    {
                        if (isCurrentSpace)
                        {
                            stringBuilder.Append(Separator);
                            addCharacter = false;
                        }
                    }
                }
            }

            if (addCharacter)
            {
                stringBuilder.Append(spanName[position]);
            }
            else
            {
                addCharacter = true;
            }
        }

        return stringBuilder.ToString().ToLowerInvariant();
    }
}
