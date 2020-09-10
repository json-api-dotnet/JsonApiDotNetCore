using System;
using JsonApiDotNetCore.Errors;

namespace JsonApiDotNetCore.Resources.Annotations
{
    // TODO: There are no tests for this.

    /// <summary>
    /// When put on a resource class, overrides global configuration for which links to render.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ResourceLinksAttribute : Attribute
    {
        private LinkTypes _topLevelLinks = LinkTypes.NotConfigured;
        private LinkTypes _resourceLinks = LinkTypes.NotConfigured;
        private LinkTypes _relationshipLinks = LinkTypes.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="TopLevelLinks"/>
        /// section for this resource.
        /// Defaults to <see cref="LinkTypes.NotConfigured"/>.
        /// </summary>
        public LinkTypes TopLevelLinks
        {
            get => _topLevelLinks;
            set
            {
                if (value == LinkTypes.Related)
                {
                    throw new InvalidConfigurationException($"{LinkTypes.Related:g} not allowed for argument {nameof(value)}");
                }

                _topLevelLinks = value;
            }
        }

        /// <summary>
        /// Configures which links to show in the <see cref="ResourceLinks"/>
        /// section for this resource.
        /// Defaults to <see cref="LinkTypes.NotConfigured"/>.
        /// </summary>
        public LinkTypes ResourceLinks
        {
            get => _resourceLinks;
            set
            {
                if (value == LinkTypes.Paging)
                {
                    throw new InvalidConfigurationException($"{LinkTypes.Paging:g} not allowed for argument {nameof(value)}");
                }

                _resourceLinks = value;
            }
        }

        /// <summary>
        /// Configures which links to show in the <see cref="RelationshipLinks"/>
        /// for all relationships of the resource type on which this attribute was used.
        /// Defaults to <see cref="LinkTypes.NotConfigured"/>.
        /// </summary>
        public LinkTypes RelationshipLinks
        {
            get => _relationshipLinks;
            set
            {
                if (value == LinkTypes.Paging)
                {
                    throw new InvalidConfigurationException($"{LinkTypes.Paging:g} not allowed for argument {nameof(value)}");
                }

                _relationshipLinks = value;
            }
        }
    }
}
