namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Used to track assignments and references to local IDs an in atomic:operations request.
    /// </summary>
    public interface ILocalIdTracker
    {
        /// <summary>
        /// Declares a local ID without assigning a server-generated value.
        /// </summary>
        void Declare(string lid, string type);

        /// <summary>
        /// Assigns a server-generated ID value to a previously declared local ID.
        /// </summary>
        void Assign(string lid, string type, string id);

        /// <summary>
        /// Gets the server-assigned ID for the specified local ID.
        /// </summary>
        string GetValue(string lid, string type);
    }
}
