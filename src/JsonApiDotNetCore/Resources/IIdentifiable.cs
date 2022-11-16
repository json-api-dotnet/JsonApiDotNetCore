namespace JsonApiDotNetCore.Resources
{
    public interface IIdentifiable
    {
        string StringId { get; set; }
        
        /// <summary>
        /// The value for element 'lid' in a JSON:API request.
        /// </summary>
        string LocalId { get; set; }
    }
    
    public interface IIdentifiable<T> : IIdentifiable
    {
        T Id { get; set; }
    }
}
