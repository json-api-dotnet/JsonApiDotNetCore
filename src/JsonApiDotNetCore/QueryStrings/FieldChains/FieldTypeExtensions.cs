using System.Text;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

internal static class FieldTypeExtensions
{
    public static void WriteTo(this FieldTypes choices, StringBuilder builder, bool pluralize, bool prefix)
    {
        int startOffset = builder.Length;

        if (choices.HasFlag(FieldTypes.ToManyRelationship) && !choices.HasFlag(FieldTypes.Relationship))
        {
            WriteChoice("to-many relationship", pluralize, prefix, false, builder, startOffset);
        }

        if (choices.HasFlag(FieldTypes.ToOneRelationship) && !choices.HasFlag(FieldTypes.Relationship))
        {
            WriteChoice("to-one relationship", pluralize, prefix, false, builder, startOffset);
        }

        if (choices.HasFlag(FieldTypes.Attribute) && !choices.HasFlag(FieldTypes.Relationship))
        {
            WriteChoice("attribute", pluralize, prefix, true, builder, startOffset);
        }

        if (choices.HasFlag(FieldTypes.Relationship) && !choices.HasFlag(FieldTypes.Field))
        {
            WriteChoice("relationship", pluralize, prefix, false, builder, startOffset);
        }

        if (choices.HasFlag(FieldTypes.Field))
        {
            WriteChoice("field", pluralize, prefix, false, builder, startOffset);
        }
    }

    private static void WriteChoice(string typeText, bool pluralize, bool prefix, bool isAttribute, StringBuilder builder, int startOffset)
    {
        if (builder.Length > startOffset)
        {
            builder.Append(" or ");
        }

        if (prefix && !pluralize)
        {
            builder.Append(isAttribute ? "an " : "a ");
        }

        builder.Append(typeText);

        if (pluralize)
        {
            builder.Append('s');
        }
    }
}
