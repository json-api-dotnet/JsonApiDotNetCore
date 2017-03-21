using System.Collections.Generic;

namespace JsonApiDotNetCore.Builders
{
    public interface IMetaBuilder
    {
        void Add(string key, object value);
        void Add(Dictionary<string,object> values);
        Dictionary<string, object> Build();
    }
}