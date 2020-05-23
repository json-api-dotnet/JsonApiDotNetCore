using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Category: Identifiable
    {
        private string _name;
        private string _description;
        
        public Category()
        {
            
        }

        public Category(string name, string description)
        {
            _name = name;
            _description = description;            
        }

        public string Name
        {
            get
            {
                return _name;
            }
            protected set
            {
                _name = value;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            protected set
            {
                _description = value;
            }
        }
    }
}
