using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// When put on a resource class, overrides global configuration for which links to render.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ResourceLinksAttribute : Attribute
    {
        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.TopLevelLinks" /> object for this resource type. Defaults to
        /// <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.TopLevelLinks" />.
        /// </summary>
        public LinkTypes TopLevelLinks { get; set; } = LinkTypes.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.ResourceLinks" /> object for this resource type. Defaults to
        /// <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.ResourceLinks" />.
        /// </summary>
        public LinkTypes ResourceLinks { get; set; } = LinkTypes.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.RelationshipLinks" /> object for all relationships of this resource type.
        /// Defaults to <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.RelationshipLinks" />. This can be overruled per
        /// relationship by setting <see cref="RelationshipAttribute.Links" />.
        /// </summary>
        public LinkTypes RelationshipLinks { get; set; } = LinkTypes.NotConfigured;
    }
}
