using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models.Fluent
{
    public class ResourceLinks: Links<ResourceLinks>
    {
        public ResourceLinks(LinksAttribute attribute): base(attribute)
        {

        }

        /// <summary>
        /// Enable self link.
        /// </summary>       
        public ResourceLinks EnableSelf(bool enable)
        {
            ToggleOption(Link.Self, enable);

            return this;
        }

        /// <summary>
        /// Enable related link.        
        /// </summary>        
        public ResourceLinks EnableRelated(bool enable)
        {
            ToggleOption(Link.Related, enable);

            return this;
        }

        protected override void EnableOption(Link link)
        {
            _attribute.ResourceLinks |= link;
        }

        protected override void DisableOption(Link link)
        {
            _attribute.ResourceLinks &= ~link;
        }
    }
}
