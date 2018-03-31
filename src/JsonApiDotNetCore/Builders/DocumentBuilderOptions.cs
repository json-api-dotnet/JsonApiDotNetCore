namespace JsonApiDotNetCore.Builders
{
    public struct DocumentBuilderOptions
    {
        public DocumentBuilderOptions(bool omitNullValuedAttributes = false)
        {
            this.OmitNullValuedAttributes = omitNullValuedAttributes;
        }

        public bool OmitNullValuedAttributes { get; private set; }
    }
}
