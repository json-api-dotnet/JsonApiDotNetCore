namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Used to track declarations, assignments and references to local IDs an in atomic:operations request.
    /// </summary>
    public interface ILocalIdTracker
    {
        /// <summary>
        /// Removes all declared and assigned values.
        /// </summary>
        void Reset();

        /// <summary>
        /// Declares a local ID without assigning a server-generated value.
        /// </summary>
        void Declare(string localId, string resourceType);

        /// <summary>
        /// Assigns a server-generated ID value to a previously declared local ID.
        /// </summary>
        void Assign(string localId, string resourceType, string stringId);

        /// <summary>
        /// Gets the server-assigned ID for the specified local ID.
        /// </summary>
        string GetValue(string localId, string resourceType);
    }
}
