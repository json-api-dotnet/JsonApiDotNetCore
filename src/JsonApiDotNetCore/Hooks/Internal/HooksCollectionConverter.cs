using System;
using System.Collections;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Hooks.Internal
{
    internal sealed class HooksCollectionConverter : CollectionConverter
    {
        public IList CopyToList(IEnumerable elements, Type elementType)
        {
            Type collectionType = typeof(List<>).MakeGenericType(elementType);
            return (IList)CopyToTypedCollection(elements, collectionType);
        }

        public IEnumerable CopyToHashSet(IEnumerable elements, Type elementType)
        {
            Type collectionType = typeof(HashSet<>).MakeGenericType(elementType);
            return CopyToTypedCollection(elements, collectionType);
        }
    }
}
