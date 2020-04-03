using System;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Models.Links
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class LinksAttribute : Attribute
    {
        public LinksAttribute(Link topLevelLinks = Link.NotConfigured, Link resourceLinks = Link.NotConfigured, Link relationshipLinks = Link.NotConfigured)
        {
            if (topLevelLinks == Link.Related)
                throw new JsonApiSetupException($"{Link.Related:g} not allowed for argument {nameof(topLevelLinks)}");

            if (resourceLinks == Link.Paging)
                throw new JsonApiSetupException($"{Link.Paging:g} not allowed for argument {nameof(resourceLinks)}");

            if (relationshipLinks == Link.Paging)
                throw new JsonApiSetupException($"{Link.Paging:g} not allowed for argument {nameof(relationshipLinks)}");

            TopLevelLinks = topLevelLinks;
            ResourceLinks = resourceLinks;
            RelationshipLinks = relationshipLinks;
        }

        /// <summary>
        /// Configures which links to show in the <see cref="TopLevelLinks"/>
        /// object for this resource.   
        /// </summary>
        public Link TopLevelLinks { get; }

        /// <summary>
        /// Configures which links to show in the <see cref="ResourceLinks"/>
        /// object for this resource.
        /// </summary>
        public Link ResourceLinks { get; }

        /// <summary>
        /// Configures which links to show in the <see cref="RelationshipLinks"/>
        /// for all relationships of the resource for which this attribute was instantiated.
        /// </summary>
        public Link RelationshipLinks { get; }
    }
}
