using System;
using JsonApiDotNetCore.Errors;

namespace JsonApiDotNetCore.Resources.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class LinksAttribute : Attribute
    {
        public LinksAttribute(Links topLevelLinks = Links.NotConfigured, Links resourceLinks = Links.NotConfigured, Links relationshipLinks = Links.NotConfigured)
        {
            if (topLevelLinks == Links.Related)
                throw new InvalidConfigurationException($"{Links.Related:g} not allowed for argument {nameof(topLevelLinks)}");

            if (resourceLinks == Links.Paging)
                throw new InvalidConfigurationException($"{Links.Paging:g} not allowed for argument {nameof(resourceLinks)}");

            if (relationshipLinks == Links.Paging)
                throw new InvalidConfigurationException($"{Links.Paging:g} not allowed for argument {nameof(relationshipLinks)}");

            TopLevelLinks = topLevelLinks;
            ResourceLinks = resourceLinks;
            RelationshipLinks = relationshipLinks;
        }

        /// <summary>
        /// Configures which links to show in the <see cref="TopLevelLinks"/>
        /// object for this resource.   
        /// </summary>
        public Links TopLevelLinks { get; }

        /// <summary>
        /// Configures which links to show in the <see cref="ResourceLinks"/>
        /// object for this resource.
        /// </summary>
        public Links ResourceLinks { get; }

        /// <summary>
        /// Configures which links to show in the <see cref="RelationshipLinks"/>
        /// for all relationships of the resource for which this attribute was instantiated.
        /// </summary>
        public Links RelationshipLinks { get; }
    }
}
