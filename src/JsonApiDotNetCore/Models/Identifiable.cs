using System;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Models
{
    public class Identifiable : Identifiable<int>
    {}
    
    public class Identifiable<T> : IIdentifiable<T>, IIdentifiable
    {
        public virtual T Id { get; set; }

        [NotMapped]
        public string StringId
        {
            get { return GetStringId(Id); }
            set { Id = (T)GetConcreteId(value); }
        }

        protected virtual string GetStringId(object value)
        {
            var type = typeof(T);
            var stringValue = value.ToString();

            if(type == typeof(Guid))
            {
                var guid = Guid.Parse(stringValue);
                return (guid == Guid.Empty ? string.Empty : stringValue);
            }

            if(stringValue == "0") return string.Empty;

            return stringValue;
        }

        protected virtual object GetConcreteId(string value)
        {
            return TypeHelper.ConvertType(value, typeof(T));
        }
    }
}
