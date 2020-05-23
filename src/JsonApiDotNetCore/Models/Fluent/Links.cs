using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models.Fluent
{
    public abstract class Links<T>
    where T: Links<T>
    {
        protected LinksAttribute _attribute;

        public Links(LinksAttribute attribute)
        {
            _attribute = attribute;
        }

        /// <summary>
        /// Disable all link options.       
        /// </summary>        
        public T DisableAll()
        {
            ToggleOption(Link.None, true);

            return (T)this;
        }

        /// <summary>
        /// Enable all link options.        
        /// </summary>        
        public T EnableAll()
        {
            ToggleOption(Link.All, true);

            return (T)this;
        }


        protected void ToggleOption(Link link, bool enable)
        {
            if (enable)
            {
                EnableOption(link);                
            }
            else
            {
                DisableOption(link);
            }            
        }

        protected abstract void EnableOption(Link link);

        protected abstract void DisableOption(Link link);        
    }
}
