using JsonApiDotNetCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore.Models
{
    public interface IResourceDefinition
    {
        List<AttrAttribute> GetOutputAttrs(object instance);
    }

    /// <summary>
    /// A scoped service used to...
    /// </summary>
    /// <typeparam name="T">The resource type</typeparam>
    public class ResourceDefinition<T> : IResourceDefinition where T : class, IIdentifiable
    {
        private readonly IContextGraph _graph;
        private readonly ContextEntity _contextEntity;
        internal readonly bool _instanceAttrsAreSpecified;

        private bool _requestCachedAttrsHaveBeenLoaded = false;
        private List<AttrAttribute> _requestCachedAttrs;

        public ResourceDefinition()
        {
            _graph = ContextGraph.Instance;
            _contextEntity = ContextGraph.Instance.GetContextEntity(typeof(T));
            _instanceAttrsAreSpecified = InstanceOutputAttrsAreSpecified();
        }

        private bool InstanceOutputAttrsAreSpecified()
        {
            var derivedType = GetType();
            var methods = derivedType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            var instanceMethod = methods
                .Where(m =>
                   m.Name == nameof(OutputAttrs)
                   && m.GetParameters()
                        .FirstOrDefault()
                        ?.ParameterType == typeof(T))
                .FirstOrDefault();
            var declaringType = instanceMethod?.DeclaringType;
            return declaringType == derivedType;
        }
        
        // TODO: need to investigate options for caching these
        protected List<AttrAttribute> Remove(Expression<Func<T, dynamic>> filter, List<AttrAttribute> from = null)
        {
            from = from ?? _contextEntity.Attributes;

            // model => model.Attribute
            if (filter.Body is MemberExpression memberExpression)
                return _contextEntity.Attributes
                        .Where(a => a.InternalAttributeName != memberExpression.Member.Name)
                        .ToList();

            // model => new { model.Attribute1, model.Attribute2 }
            if (filter.Body is NewExpression newExpression)
            {
                var attributes = new List<AttrAttribute>();
                foreach (var attr in _contextEntity.Attributes)
                    if (newExpression.Members.Any(m => m.Name == attr.InternalAttributeName) == false)
                        attributes.Add(attr);

                return attributes;
            }

            throw new JsonApiException(500,
                message: $"The expression returned by '{filter}' for '{GetType()}' is of type {filter.Body.GetType()}"
                        + " and cannot be used to select resource attributes. ",
                detail: "The type must be a NewExpression. Example: article => new { article.Author }; ");
        }

        /// <summary>
        /// Allows POST / PATCH requests to set the value of an
        /// attribute, but exclude the attribute in the response
        /// this might be used if the incoming value gets hashed or
        /// encrypted prior to being persisted and this value should
        /// never be sent back to the client.
        ///
        /// Called once per filtered resource in request.
        /// </summary>
        protected virtual List<AttrAttribute> OutputAttrs() => _contextEntity.Attributes;

        /// <summary>
        /// Allows POST / PATCH requests to set the value of an
        /// attribute, but exclude the attribute in the response
        /// this might be used if the incoming value gets hashed or
        /// encrypted prior to being persisted and this value should
        /// never be sent back to the client.
        ///
        /// Called for every instance of a resource.
        /// </summary>
        protected virtual List<AttrAttribute> OutputAttrs(T instance) => _contextEntity.Attributes;

        public List<AttrAttribute> GetOutputAttrs(object instance)
            => _instanceAttrsAreSpecified == false
                ? GetOutputAttrs()
                : OutputAttrs(instance as T);

        private List<AttrAttribute> GetOutputAttrs()
        {
            if (_requestCachedAttrsHaveBeenLoaded == false)
            {
                _requestCachedAttrs = OutputAttrs();
                // the reason we don't just check for null is because we
                // guarantee that OutputAttrs will be called once per
                // request and null is a valid return value
                _requestCachedAttrsHaveBeenLoaded = true;
            }

            return _requestCachedAttrs;
        }
    }
}
