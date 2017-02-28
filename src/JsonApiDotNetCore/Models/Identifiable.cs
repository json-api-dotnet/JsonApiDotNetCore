namespace JsonApiDotNetCore.Models
{
    public abstract class Identifiable<T> : IIdentifiable<T>, IIdentifiable
    {
        public T Id { get; set; }

        object IIdentifiable.Id
        {
            get { return Id; }
            set { Id = (T)value; }
        }
    }
}
