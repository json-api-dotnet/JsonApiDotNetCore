using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Builders
{
    public class DocumentBuilderOptionsProvider : IDocumentBuilderOptionsProvider
    {
        public DocumentBuilderOptionsProvider(IJsonApiOptions options)
        {
        }

        public SerializerBehaviour GetDocumentBuilderOptions()
        {
            var nullAttributeResponseBehaviorConfig = this._jsonApiContext.Options.NullAttributeResponseBehavior;
            if (nullAttributeResponseBehaviorConfig.AllowClientOverride && _httpContextAccessor.HttpContext.Request.Query.TryGetValue("omitNullValuedAttributes", out var omitNullValuedAttributesQs))
            {
                if (bool.TryParse(omitNullValuedAttributesQs, out var omitNullValuedAttributes))
                {
                    //return new SerializerBehaviour(omitNullValuedAttributes);
                    return null;
                }
            }
            //return new SerializerBehaviour(this._jsonApiContext.Options.NullAttributeResponseBehavior.OmitNullValuedAttributes);

            return null;
        }
    }

    public interface IDocumentBuilderOptionsProvider
    {
    }
}
