using System;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Models
{
    public class Identifiable : Identifiable<int>
    {}
    
    public class Identifiable<T> : IIdentifiable<T>
    {
        public virtual T Id { get; set; }

        [NotMapped]
        public string StringId
        {
            get => GetStringId(Id);
            set => Id = GetTypedId(value);
        }

        protected virtual string GetStringId(object value)
        {
            var type = typeof(T);
            var stringValue = value.ToString();

            if(type == typeof(Guid))
            {
                var guid = Guid.Parse(stringValue);
                return guid == Guid.Empty ? string.Empty : stringValue;
            }

            return stringValue == "0" 
                ? string.Empty 
                : stringValue;
        }

        protected virtual T GetTypedId(string value)
        {
            var convertedValue = TypeHelper.ConvertType(value, typeof(T));
            return convertedValue == null ? default : (T)convertedValue;
        }

        [Obsolete("Use GetTypedId instead")]
        protected virtual object GetConcreteId(string value)
        {
            return TypeHelper.ConvertType(value, typeof(T));
        }
    }
}
