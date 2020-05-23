using System;

namespace JsonApiDotNetCore.Models.Fluent
{
    public class HasManyThrough
    {
        private HasManyThroughAttribute _attribute;

        public HasManyThrough(HasManyThroughAttribute attribute)
        {
            _attribute = attribute;
        }

        /// <summary>
        /// Exposes a relationship with an explicit name.
        /// </summary>  
        /// <param name="publicName">The relationship name as exposed by the API</param>        
        public HasManyThrough PublicName(string publicName)
        {
            if (publicName == null)
            {
                throw new ArgumentNullException(nameof(publicName));
            }

            if (string.IsNullOrWhiteSpace(publicName))
            {
                throw new ArgumentException("Exposed name cannot be empty or contain only whitespace.", nameof(publicName));
            }

            _attribute.PublicRelationshipName = publicName;

            return this;
        }
    }
}
