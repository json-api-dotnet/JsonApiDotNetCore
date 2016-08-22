using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Attributes;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiModelConfiguration : IJsonApiModelConfiguration
    {
        private string _namespace;
        private Dictionary<string, Type> _routes;
        private Type _contextType;

        public JsonApiModelConfiguration()
        {
          _routes = new Dictionary<string, Type>();
        }

        public void UseContext<T>()
        {
          _contextType = typeof(T);
        }

        public void SetDefaultNamespace(string ns)
        {
            _namespace = ns;
        }

        public void AddModel<T>()
        {
            var t = typeof(T);
            _routes.Add(BuildRoute(t), t);
        }

        public Type GetTypeForRoute(string route)
        {
            Type t;
            return _routes.TryGetValue(route, out t) ? t : null;;
        }

        private string BuildRoute(Type type)
        {
          return $"{_namespace}/{GetModelRouteName(type)}";
        }

        private string GetModelRouteName(Type type)
        {
          var attributes = TypeDescriptor.GetAttributes(type);
          return GetPluralNameFromAttributes(attributes);
        }

        private static string GetPluralNameFromAttributes(AttributeCollection attributes)
        {
          return ((SerializationFormat)attributes[typeof(SerializationFormat)])?.PluralName;
        }
    }
}
