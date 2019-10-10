using System;

namespace JsonApiDotNetCore.Query
{
    public class OmitNullService : QueryParameterService
    {
        public override void Parse(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}
