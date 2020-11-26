namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Used to track assignments and references to local IDs an in atomic:operations request.
    /// </summary>
    public interface ILocalIdTracker
    {
        /// <summary>
        /// Assigns a server-generated value to a local ID.
        /// </summary>
        void AssignValue(string lid, string id);

        /// <summary>
        /// Gets the server-assigned ID for the specified local ID.
        /// </summary>
        string GetAssignedValue(string lid);

        /// <summary>
        /// Indicates whether a server-generated value is available for the specified local ID.
        /// </summary>
        bool IsAssigned(string lid);
    }
}
