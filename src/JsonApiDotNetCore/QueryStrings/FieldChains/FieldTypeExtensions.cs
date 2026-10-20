using System.Text;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

internal static class FieldTypeExtensions
{
    public static void WriteTo(this FieldTypes choices, StringBuilder builder, bool pluralize, bool prefix)
    {
        var writer = new ChoiceWriter(builder, pluralize, prefix);

        if (choices.HasFlag(FieldTypes.Field))
        {
            writer.Write("field");
        }
        else
        {
            WriteRelationships(choices, writer);
            WriteAttributes(choices, writer);
        }
    }

    private static void WriteRelationships(FieldTypes choices, ChoiceWriter writer)
    {
        if (choices.HasFlag(FieldTypes.Relationship))
        {
            writer.Write("relationship");
        }
        else if (choices.HasFlag(FieldTypes.ToOneRelationship))
        {
            writer.Write("to-one relationship");
        }
        else if (choices.HasFlag(FieldTypes.ToManyRelationship))
        {
            writer.Write("to-many relationship");
        }
    }

    private static void WriteAttributes(FieldTypes choices, ChoiceWriter writer)
    {
        if (choices.HasFlag(FieldTypes.Attribute))
        {
            writer.Write("attribute");
        }
        else
        {
            bool hasScalar = choices.HasFlag(FieldTypes.ScalarAttribute);
            bool hasCompound = choices.HasFlag(FieldTypes.CompoundAttribute);
            bool hasScalarCollection = choices.HasFlag(FieldTypes.ScalarCollectionAttribute);
            bool hasCompoundCollection = choices.HasFlag(FieldTypes.CompoundCollectionAttribute);

            if (hasScalar && hasCompound && !hasScalarCollection && !hasCompoundCollection)
            {
                writer.Write("non-collection attribute");
            }
            else if (hasScalarCollection && hasCompoundCollection && !hasScalar && !hasCompound)
            {
                writer.Write("collection attribute");
            }
            else
            {
                WriteScalarAttributes(hasScalar, hasScalarCollection, writer);
                WriteCompoundAttributes(hasCompound, hasCompoundCollection, writer);
            }
        }
    }

    private static void WriteScalarAttributes(bool hasScalar, bool hasScalarCollection, ChoiceWriter writer)
    {
        if (hasScalar && hasScalarCollection)
        {
            writer.Write("scalar attribute");
        }
        else if (hasScalar)
        {
            writer.Write("scalar non-collection attribute");
        }
        else if (hasScalarCollection)
        {
            writer.Write("scalar collection attribute");
        }
    }

    private static void WriteCompoundAttributes(bool hasCompound, bool hasCompoundCollection, ChoiceWriter writer)
    {
        if (hasCompound && hasCompoundCollection)
        {
            writer.Write("compound attribute");
        }
        else if (hasCompound)
        {
            writer.Write("compound non-collection attribute");
        }
        else if (hasCompoundCollection)
        {
            writer.Write("compound collection attribute");
        }
    }

    private readonly ref struct ChoiceWriter(StringBuilder builder, bool pluralize, bool prefix)
    {
        private readonly int _startOffset = builder.Length;

        public void Write(string choice)
        {
            if (builder.Length > _startOffset)
            {
                builder.Append(" or ");
            }

            if (prefix && !pluralize)
            {
                builder.Append(choice.StartsWith('a') ? "an " : "a ");
            }

            builder.Append(choice);

            if (pluralize)
            {
                builder.Append('s');
            }
        }
    }
}
