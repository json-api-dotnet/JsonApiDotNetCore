using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Logic
{
    public interface ILogicCache
    {
        IResourceDefinition GetLogic(Type type);
    }
    public class BaseLogicCache : ILogicCache
    {
        protected Dictionary<Type, IResourceDefinition> _cache = new Dictionary<Type, IResourceDefinition>();


        public IResourceDefinition GetLogic(Type type) 
        {
            var toReturn = _cache.FirstOrDefault(c => c.Key == type); 
            if(toReturn.Equals(default(KeyValuePair<Type,IResourceDefinition>)))
            {
                return null;
            }
            return  toReturn.Value;
        }
    }

}
