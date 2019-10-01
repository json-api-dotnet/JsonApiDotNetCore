namespace JsonApiDotNetCore.QueryServices.Contracts
{
    public interface IAttributeBehaviourQuery
    {
        bool? OmitNullValuedAttributes { get; set; }
        bool? OmitDefaultValuedAttributes { get; set; }
    }
}
