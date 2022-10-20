namespace JsonApiDotNetCore.Resources.Annotations;

internal static class CapabilitiesExtensions
{
    public static bool IsViewBlocked(this ResourceFieldAttribute field)
    {
        return field switch
        {
            AttrAttribute attrAttribute => !attrAttribute.Capabilities.HasFlag(AttrCapabilities.AllowView),
            HasOneAttribute hasOneRelationship => !hasOneRelationship.Capabilities.HasFlag(HasOneCapabilities.AllowView),
            HasManyAttribute hasManyRelationship => !hasManyRelationship.Capabilities.HasFlag(HasManyCapabilities.AllowView),
            _ => false
        };
    }

    public static bool IsIncludeBlocked(this RelationshipAttribute relationship)
    {
        return relationship switch
        {
            HasOneAttribute hasOneRelationship => !hasOneRelationship.Capabilities.HasFlag(HasOneCapabilities.AllowInclude),
            HasManyAttribute hasManyRelationship => !hasManyRelationship.Capabilities.HasFlag(HasManyCapabilities.AllowInclude),
            _ => false
        };
    }

    public static bool IsFilterBlocked(this ResourceFieldAttribute field)
    {
        return field switch
        {
            AttrAttribute attrAttribute => !attrAttribute.Capabilities.HasFlag(AttrCapabilities.AllowFilter),
            HasManyAttribute hasManyRelationship => !hasManyRelationship.Capabilities.HasFlag(HasManyCapabilities.AllowFilter),
            _ => false
        };
    }

    public static bool IsSetBlocked(this RelationshipAttribute relationship)
    {
        return relationship switch
        {
            HasOneAttribute hasOneRelationship => !hasOneRelationship.Capabilities.HasFlag(HasOneCapabilities.AllowSet),
            HasManyAttribute hasManyRelationship => !hasManyRelationship.Capabilities.HasFlag(HasManyCapabilities.AllowSet),
            _ => false
        };
    }
}
