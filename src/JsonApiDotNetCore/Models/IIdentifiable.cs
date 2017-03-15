namespace JsonApiDotNetCore.Models
{
    public interface IIdentifiable
    {
        object Id { get; set; }
        bool IsIdEmpty();
    }

    public interface IIdentifiable<T> : IIdentifiable
    {
        new T Id { get; set; }
    }
}
