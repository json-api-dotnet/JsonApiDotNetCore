namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Indicates how to override sparse fieldset selection coming from constraints.
    /// </summary>
    public enum TopFieldSelection
    {
        /// <summary>
        /// Preserves the existing selection of attributes and/or relationships.
        /// </summary>
        PreserveExisting,

        /// <summary>
        /// Preserves included relationships, but selects all resource attributes.
        /// </summary>
        WithAllAttributes,

        /// <summary>
        /// Discards any included relationships and selects only resource ID.
        /// </summary>
        OnlyIdAttribute
    }
}
