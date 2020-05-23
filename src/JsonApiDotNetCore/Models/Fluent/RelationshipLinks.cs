using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models.Fluent
{
    public class RelationshipLinks: Links<RelationshipLinks>
    {

        public RelationshipLinks(LinksAttribute attribute): base(attribute)
        {

        }

        /// <summary>
        /// Enable self link.
        /// </summary>       
        public RelationshipLinks EnableSelf(bool enable)
        {
            ToggleOption(Link.Self, enable);

            return this;
        }

        /// <summary>
        /// Enable related link.        
        /// </summary>        
        public RelationshipLinks EnableRelated(bool enable)
        {
            ToggleOption(Link.Related, enable);

            return this;
        }

        protected override void EnableOption(Link link)
        {
            _attribute.RelationshipLinks |= link;
        }

        protected override void DisableOption(Link link)
        {
            _attribute.RelationshipLinks &= ~link;
        }
    }
}
