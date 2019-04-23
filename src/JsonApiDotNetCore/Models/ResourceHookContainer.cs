using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Models
{
    abstract public class ResourceHookContainer<T> : IResourceHookContainer<T> where T : class, IIdentifiable
    {


        protected readonly ResourceHook[] _implementedHooks;
        public ResourceHookContainer(IImplementedResourceHooks<T> implementedResourceHooks = null)
        {
            _implementedHooks = implementedResourceHooks?.ImplementedHooks;
        }
        /// <inheritdoc/>
        public virtual bool ShouldExecuteHook(ResourceHook hook)
        {
            return _implementedHooks.Contains(hook);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> BeforeCreate(IEnumerable<T> entities, ResourceAction actionSource)
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> AfterCreate(IEnumerable<T> entities, ResourceAction actionSource)
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeRead(ResourceAction actionSource, string stringId = null)
        {
            return;
        }

        /// <inheritdoc/>
        public abstract  IEnumerable<T> AfterRead(IEnumerable<T> entities, ResourceAction actionSource);

        /// <inheritdoc/>
        public virtual IEnumerable<T> BeforeUpdate(IEnumerable<T> entities, ResourceAction actionSource)
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> AfterUpdate(IEnumerable<T> entities, ResourceAction actionSource)
        {
            return entities;
        }

        /// <inheritdoc/>
        public virtual void BeforeDelete(IEnumerable<T> entities, ResourceAction actionSource)
        {
            return;
        }

        /// <inheritdoc/>
        public virtual void AfterDelete(IEnumerable<T> entities, bool succeeded, ResourceAction actionSource)
        {
            return;
        }
    }
}
