namespace JsonApiDotNetCore.Models
{
    public class Identifiable : Identifiable<int>
    {}
    
    public class Identifiable<T> : IIdentifiable<T>, IIdentifiable
    {
        public T Id { get; set; }

        object IIdentifiable.Id
        {
            get { return Id; }
            set { Id = (T)value; }
        }
    }
}
