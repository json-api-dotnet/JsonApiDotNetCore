using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Metadata about the shape of a JSON:API resource in the resource graph.
    /// </summary>
    [PublicAPI]
    public sealed class ResourceType
    {
        private readonly Dictionary<string, ResourceFieldAttribute> _fieldsByPublicName = new();
        private readonly Dictionary<string, ResourceFieldAttribute> _fieldsByPropertyName = new();

        /// <summary>
        /// The publicly exposed resource name.
        /// </summary>
        public string PublicName { get; }

        /// <summary>
        /// The CLR type of the resource.
        /// </summary>
        public Type ClrType { get; }

        /// <summary>
        /// The CLR type of the resource identity.
        /// </summary>
        public Type IdentityClrType { get; }

        /// <summary>
        /// Exposed resource attributes and relationships. See https://jsonapi.org/format/#document-resource-object-fields.
        /// </summary>
        public IReadOnlyCollection<ResourceFieldAttribute> Fields { get; }

        /// <summary>
        /// Exposed resource attributes. See https://jsonapi.org/format/#document-resource-object-attributes.
        /// </summary>
        public IReadOnlyCollection<AttrAttribute> Attributes { get; }

        /// <summary>
        /// Exposed resource relationships. See https://jsonapi.org/format/#document-resource-object-relationships.
        /// </summary>
        public IReadOnlyCollection<RelationshipAttribute> Relationships { get; }

        /// <summary>
        /// Related entities that are not exposed as resource relationships.
        /// </summary>
        public IReadOnlyCollection<EagerLoadAttribute> EagerLoads { get; }

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.TopLevelLinks" /> object for this resource type. Defaults to
        /// <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.TopLevelLinks" />.
        /// </summary>
        /// <remarks>
        /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.TopLevelLinks" /> usage.
        /// </remarks>
        public LinkTypes TopLevelLinks { get; }

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.ResourceLinks" /> object for this resource type. Defaults to
        /// <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.ResourceLinks" />.
        /// </summary>
        /// <remarks>
        /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.ResourceLinks" /> usage.
        /// </remarks>
        public LinkTypes ResourceLinks { get; }

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.RelationshipLinks" /> object for all relationships of this resource type.
        /// Defaults to <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.RelationshipLinks" />. This can be overruled per
        /// relationship by setting <see cref="RelationshipAttribute.Links" />.
        /// </summary>
        /// <remarks>
        /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.RelationshipLinks" /> usage.
        /// </remarks>
        public LinkTypes RelationshipLinks { get; }

        public ResourceType(string publicName, Type clrType, Type identityClrType, IReadOnlyCollection<AttrAttribute>? attributes = null,
            IReadOnlyCollection<RelationshipAttribute>? relationships = null, IReadOnlyCollection<EagerLoadAttribute>? eagerLoads = null,
            LinkTypes topLevelLinks = LinkTypes.NotConfigured, LinkTypes resourceLinks = LinkTypes.NotConfigured,
            LinkTypes relationshipLinks = LinkTypes.NotConfigured)
        {
            ArgumentGuard.NotNullNorEmpty(publicName, nameof(publicName));
            ArgumentGuard.NotNull(clrType, nameof(clrType));
            ArgumentGuard.NotNull(identityClrType, nameof(identityClrType));

            PublicName = publicName;
            ClrType = clrType;
            IdentityClrType = identityClrType;
            Attributes = attributes ?? Array.Empty<AttrAttribute>();
            Relationships = relationships ?? Array.Empty<RelationshipAttribute>();
            EagerLoads = eagerLoads ?? Array.Empty<EagerLoadAttribute>();
            TopLevelLinks = topLevelLinks;
            ResourceLinks = resourceLinks;
            RelationshipLinks = relationshipLinks;
            Fields = Attributes.Cast<ResourceFieldAttribute>().Concat(Relationships).ToArray();

            foreach (ResourceFieldAttribute field in Fields)
            {
                _fieldsByPublicName.Add(field.PublicName, field);
                _fieldsByPropertyName.Add(field.Property.Name, field);
            }
        }

        public AttrAttribute GetAttributeByPublicName(string publicName)
        {
            AttrAttribute? attribute = FindAttributeByPublicName(publicName);
            return attribute ?? throw new InvalidOperationException($"Attribute '{publicName}' does not exist on resource type '{PublicName}'.");
        }

        public AttrAttribute? FindAttributeByPublicName(string publicName)
        {
            ArgumentGuard.NotNull(publicName, nameof(publicName));

            return _fieldsByPublicName.TryGetValue(publicName, out ResourceFieldAttribute? field) && field is AttrAttribute attribute ? attribute : null;
        }

        public AttrAttribute GetAttributeByPropertyName(string propertyName)
        {
            AttrAttribute? attribute = FindAttributeByPropertyName(propertyName);

            return attribute ??
                throw new InvalidOperationException($"Attribute for property '{propertyName}' does not exist on resource type '{ClrType.Name}'.");
        }

        public AttrAttribute? FindAttributeByPropertyName(string propertyName)
        {
            ArgumentGuard.NotNull(propertyName, nameof(propertyName));

            return _fieldsByPropertyName.TryGetValue(propertyName, out ResourceFieldAttribute? field) && field is AttrAttribute attribute ? attribute : null;
        }

        public RelationshipAttribute GetRelationshipByPublicName(string publicName)
        {
            RelationshipAttribute? relationship = FindRelationshipByPublicName(publicName);
            return relationship ?? throw new InvalidOperationException($"Relationship '{publicName}' does not exist on resource type '{PublicName}'.");
        }

        public RelationshipAttribute? FindRelationshipByPublicName(string publicName)
        {
            ArgumentGuard.NotNull(publicName, nameof(publicName));

            return _fieldsByPublicName.TryGetValue(publicName, out ResourceFieldAttribute? field) && field is RelationshipAttribute relationship
                ? relationship
                : null;
        }

        public RelationshipAttribute GetRelationshipByPropertyName(string propertyName)
        {
            RelationshipAttribute? relationship = FindRelationshipByPropertyName(propertyName);

            return relationship ??
                throw new InvalidOperationException($"Relationship for property '{propertyName}' does not exist on resource type '{ClrType.Name}'.");
        }

        public RelationshipAttribute? FindRelationshipByPropertyName(string propertyName)
        {
            ArgumentGuard.NotNull(propertyName, nameof(propertyName));

            return _fieldsByPropertyName.TryGetValue(propertyName, out ResourceFieldAttribute? field) && field is RelationshipAttribute relationship
                ? relationship
                : null;
        }

        public override string ToString()
        {
            return PublicName;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (ResourceType)obj;

            return PublicName == other.PublicName && ClrType == other.ClrType && IdentityClrType == other.IdentityClrType &&
                Attributes.SequenceEqual(other.Attributes) && Relationships.SequenceEqual(other.Relationships) && EagerLoads.SequenceEqual(other.EagerLoads) &&
                TopLevelLinks == other.TopLevelLinks && ResourceLinks == other.ResourceLinks && RelationshipLinks == other.RelationshipLinks;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(PublicName);
            hashCode.Add(ClrType);
            hashCode.Add(IdentityClrType);

            foreach (AttrAttribute attribute in Attributes)
            {
                hashCode.Add(attribute);
            }

            foreach (RelationshipAttribute relationship in Relationships)
            {
                hashCode.Add(relationship);
            }

            foreach (EagerLoadAttribute eagerLoad in EagerLoads)
            {
                hashCode.Add(eagerLoad);
            }

            hashCode.Add(TopLevelLinks);
            hashCode.Add(ResourceLinks);
            hashCode.Add(RelationshipLinks);

            return hashCode.ToHashCode();
        }
    }
}
