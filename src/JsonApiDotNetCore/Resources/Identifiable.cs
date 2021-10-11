#nullable disable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources.Internal;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// A convenient basic implementation of <see cref="IIdentifiable{TId}" /> that provides conversion between typed <see cref="Id" /> and
    /// <see cref="StringId" />.
    /// </summary>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    public abstract class Identifiable<TId> : IIdentifiable<TId>
    {
        /// <inheritdoc />
        public virtual TId Id { get; set; }

        /// <inheritdoc />
        [NotMapped]
        public string StringId
        {
            get => GetStringId(Id);
            set => Id = GetTypedId(value);
        }

        /// <inheritdoc />
        [NotMapped]
        public string LocalId { get; set; }

        /// <summary>
        /// Converts an outgoing typed resource identifier to string format for use in a JSON:API response.
        /// </summary>
        protected virtual string GetStringId(TId value)
        {
            return EqualityComparer<TId>.Default.Equals(value, default) ? null : value.ToString();
        }

        /// <summary>
        /// Converts an incoming 'id' element from a JSON:API request to the typed resource identifier.
        /// </summary>
        protected virtual TId GetTypedId(string value)
        {
            return value == null ? default : (TId)RuntimeTypeConverter.ConvertType(value, typeof(TId));
        }
    }
}
