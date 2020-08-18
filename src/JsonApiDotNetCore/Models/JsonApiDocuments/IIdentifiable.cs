namespace JsonApiDotNetCore.Models
{
    public interface IIdentifiable
    {
        string StringId { get; set; }
    }

    public interface IIdentifiable<TId> : IIdentifiable
    {
        TId Id { get; set; }
    }
}
