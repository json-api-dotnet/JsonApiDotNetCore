using System.Collections.Generic;
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
        protected virtual string GetStringId(TId value)
        {
            return EqualityComparer<TId>.Default.Equals(value, default) ? null : value.ToString();
        }

        /// <summary>
        /// Convert a string to a typed resource identifier.
        /// </summary>
        protected virtual TId GetTypedId(string value)
        {
            return string.IsNullOrEmpty(value) ? default : (TId)TypeHelper.ConvertType(value, typeof(TId));
        }
    }
}
