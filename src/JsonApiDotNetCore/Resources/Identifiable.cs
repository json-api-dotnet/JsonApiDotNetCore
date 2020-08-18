using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonApiDotNetCore.Resources
{
    public abstract class Identifiable : Identifiable<int>
    { }

    public abstract class Identifiable<TId> : IIdentifiable<TId>
    {
        /// <summary>
        /// The resource identifier
        /// </summary>
        public virtual TId Id { get; set; }

        /// <summary>
        /// The string representation of the `Id`.
        /// 
        /// This is used in serialization and deserialization.
        /// The getters should handle the conversion
        /// from `typeof(TId)` to a string and the setter vice versa.
        /// 
        /// To override this behavior, you can either implement the
        /// <see cref="IIdentifiable{TId}" /> interface directly or override
        /// `GetStringId` and `GetTypedId` methods.
        /// </summary>
        [NotMapped]
        public string StringId
        {
            get => GetStringId(Id);
            set => Id = GetTypedId(value);
        }

        /// <summary>
        /// Convert the provided resource identifier to a string.
        /// </summary>
        protected virtual string GetStringId(object value)
        {
            if(value == null)
                return string.Empty; // todo; investigate why not using null, because null would make more sense in serialization

            var type = typeof(TId);
            var stringValue = value.ToString();

            if (type == typeof(Guid))
            {
                var guid = Guid.Parse(stringValue);
                return guid == Guid.Empty ? string.Empty : stringValue;
            }

            return stringValue == "0"
                ? string.Empty
                : stringValue;
        }

        /// <summary>
        /// Convert a string to a typed resource identifier.
        /// </summary>
        protected virtual TId GetTypedId(string value)
        {
            if (value == null)
                return default;
            return (TId)TypeHelper.ConvertType(value, typeof(TId));
        }
    }
}
