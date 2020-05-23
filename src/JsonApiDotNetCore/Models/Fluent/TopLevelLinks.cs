using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models.Fluent
{
    public class TopLevelLinks: Links<TopLevelLinks>
    {
        public TopLevelLinks(LinksAttribute attribute): base(attribute)
        {

        }

        /// <summary>
        /// Enable self link.
        /// </summary>       
        public TopLevelLinks EnableSelf(bool enable)
        {
            ToggleOption(Link.Self, enable);

            return this;
        }

        /// <summary>
        /// Enable paging links.        
        /// </summary>        
        public TopLevelLinks EnablePaging(bool enable)
        {
            ToggleOption(Link.Paging, enable);

            return this;
        }

        protected override void EnableOption(Link link)
        {
            _attribute.TopLevelLinks |= link;
        }

        protected override void DisableOption(Link link)
        {
            _attribute.TopLevelLinks &= ~link;
        }
    }
}
