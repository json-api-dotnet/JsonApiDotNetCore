using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public interface IMetaBuilder<T> where T : class, IIdentifiable
    {
        void Add(string key, object value);
        void Add(Dictionary<string,object> values);
        Dictionary<string, object> GetMeta();
    }
}