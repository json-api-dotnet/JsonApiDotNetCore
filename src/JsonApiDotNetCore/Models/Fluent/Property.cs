using System;

namespace JsonApiDotNetCore.Models.Fluent
{
    public class Property
    {
        private AttrAttribute _attribute;

        public Property(AttrAttribute attribute)
        {
            _attribute = attribute;
        }

        /// <summary>
        /// Exposes a property with an explicit name.
        /// </summary>  
        /// <param name="publicName">The property name as exposed by the API</param>        
        public Property PublicName(string publicName)
        {
            if (publicName == null)
            {
                throw new ArgumentNullException(nameof(publicName));
            }

            if (string.IsNullOrWhiteSpace(publicName))
            {
                throw new ArgumentException("Exposed name cannot be empty or contain only whitespace.", nameof(publicName));
            }

            _attribute.PublicAttributeName = publicName;

            return this;
        }

        /// <summary>
        /// Allow none of the capabilities.
        /// </summary>       
        public Property AllowNone()
        {
            ToggleCapability(AttrCapabilities.None, true);

            return this;
        }

        /// <summary>
        /// Whether or not PATCH requests can update the attribute value.
        /// Attempts to update when disabled will return an HTTP 422 response.
        /// </summary>        
        public Property AllowMutate(bool allow)
        {
            ToggleCapability(AttrCapabilities.AllowMutate, allow);

            return this;
        }

        /// <summary>
        /// Whether or not an attribute can be filtered on via a query string parameter.
        /// Attempts to sort when disabled will return an HTTP 400 response.
        /// </summary>       
        public Property AllowFilter(bool allow)
        {
            ToggleCapability(AttrCapabilities.AllowFilter, allow);

            return this;
        }

        /// <summary>
        /// Whether or not an attribute can be sorted on via a query string parameter.
        /// Attempts to sort when disabled will return an HTTP 400 response.
        /// </summary>        
        public Property AllowSort(bool allow)
        {
            ToggleCapability(AttrCapabilities.AllowSort, allow);

            return this;
        }

        /// <summary>
        /// Allow all capabilities.
        /// </summary>        
        public Property AllowAll()
        {
            ToggleCapability(AttrCapabilities.All, true);

            return this;
        }

        protected void ToggleCapability(AttrCapabilities capability, bool allow)
        {
            if (allow)
            {
                EnableCapability(capability);
            }
            else
            {
                DisableCapability(capability);
            }

            _attribute.HasExplicitCapabilities = true;
        }

        protected void EnableCapability(AttrCapabilities capability)
        {
            _attribute.Capabilities |= capability;
        }

        protected void DisableCapability(AttrCapabilities capability)
        {
            _attribute.Capabilities &= ~capability;
        }
    }
}
