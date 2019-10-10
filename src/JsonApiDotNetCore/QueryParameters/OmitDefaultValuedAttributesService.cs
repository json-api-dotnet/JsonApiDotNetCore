using System;

namespace JsonApiDotNetCore.Query
{
    public class OmitDefaultService : QueryParameterService
    {
        public override void Parse(string value)
        {
            throw new NotImplementedException();
        }
    }
}
