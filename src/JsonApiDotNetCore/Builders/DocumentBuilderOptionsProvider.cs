using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Builders
{
    public class DocumentBuilderOptionsProvider : IDocumentBuilderOptionsProvider
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentBuilderOptionsProvider(IJsonApiContext jsonApiContext, IHttpContextAccessor httpContextAccessor)
        {
            _jsonApiContext = jsonApiContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public DocumentBuilderOptions GetDocumentBuilderOptions()
        {
            var nullAttributeResponseBehaviorConfig = this._jsonApiContext.Options.NullAttributeResponseBehavior;
            if (nullAttributeResponseBehaviorConfig.AllowClientOverride && _httpContextAccessor.HttpContext.Request.Query.TryGetValue("omitNullValuedAttributes", out var omitNullValuedAttributesQs))
            {
                if (bool.TryParse(omitNullValuedAttributesQs, out var omitNullValuedAttributes))
                {
                    return new DocumentBuilderOptions(omitNullValuedAttributes);                                
                }
            }
            return new DocumentBuilderOptions(this._jsonApiContext.Options.NullAttributeResponseBehavior.OmitNullValuedAttributes);
        }
    }
}
