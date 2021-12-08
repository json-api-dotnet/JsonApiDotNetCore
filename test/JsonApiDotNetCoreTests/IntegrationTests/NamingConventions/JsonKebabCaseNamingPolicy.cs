using System.Text;
using System.Text.Json;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

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
                    isNextLower = spanName[position + 1] > 96 && spanName[position + 1] < 123;
                    isNextUpper = spanName[position + 1] > 64 && spanName[position + 1] < 91;
                    isNextSpace = spanName[position + 1] == 32;
                }

                if (isCurrentSpace && (isPreviousSpace || isPreviousSeparator || isNextUpper || isNextSpace))
                {
                    addCharacter = false;
                }
                else
                {
                    bool isCurrentUpper = spanName[position] > 64 && spanName[position] < 91;
                    bool isPreviousLower = spanName[position - 1] > 96 && spanName[position - 1] < 123;
                    bool isPreviousNumber = spanName[position - 1] > 47 && spanName[position - 1] < 58;

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

        return stringBuilder.ToString().ToLower();
    }
}
