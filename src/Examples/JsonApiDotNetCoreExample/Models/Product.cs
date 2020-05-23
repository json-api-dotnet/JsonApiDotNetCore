using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using System.Collections.Generic;

namespace JsonApiDotNetCoreExample.Models
{    
    public class Product: Identifiable
    {
        private string _name;
        private string _description;
        private Money _price;
        private List<Category> _categories;

        public Product()
        {
            _price = new Money(0, "USD");
            _categories = new List<Category>();
        }

        public Product(string name, string description, Money price)
        {
            _name = name;
            _description = description;
            _price = price;
            _categories = new List<Category>();
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

        public Money Price
        {
            get
            {
                return _price;
            }
            protected set
            {
                _price = value;
            }
        }

        //public IReadOnlyList<Category> Categories
        //{
        //    get
        //    {
        //        return _categories;
        //    }            
        //}

        public List<Category> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                _categories = value;
            }
        }

        public void AddCategory(Category category)
        {
            _categories.Add(category);
        }

        public void ChangePrice(Money price)
        {
            _price = price;
        }
    }
}
