namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    public sealed class RelatedIdMapper : IRelatedIdMapper
    {
        /// <inheritdoc />
        public string GetRelatedIdPropertyName(string propertyName) => propertyName + "Id";
    }
}
