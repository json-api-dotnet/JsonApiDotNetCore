namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Parses a dot-separated resource field chain from text into a list of field names.
/// </summary>
internal sealed class FieldChainParser
{
    public IEnumerable<string> Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.Length > 0)
        {
            var fields = new List<string>(source.Split('.'));
            int position = 0;

            foreach (string field in fields)
            {
                string trimmed = field.Trim();

                if (field.Length == 0 || trimmed.Length != field.Length)
                {
                    throw new FieldChainFormatException(position, "Field name expected.");
                }

                position += field.Length + 1;
            }

            return fields;
        }

        return Array.Empty<string>();
    }
}
