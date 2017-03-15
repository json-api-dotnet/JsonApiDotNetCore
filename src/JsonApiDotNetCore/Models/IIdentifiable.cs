namespace JsonApiDotNetCore.Models
{
    public interface IIdentifiable
    {
        string Id { get; set; }
    }

    public interface IIdentifiable<T> : IIdentifiable
    {
        new T Id { get; set; }
    }
}
