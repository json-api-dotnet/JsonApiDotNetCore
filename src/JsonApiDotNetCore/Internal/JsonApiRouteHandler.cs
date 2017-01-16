// REF: https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.Core/Internal/MvcRouteHandler.cs
using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;

namespace JsonApiDotNetCore.Internal
{
    public class JsonApiRouteHandler : IRouter
    {
        private IActionContextAccessor _actionContextAccessor;
        private IActionInvokerFactory _actionInvokerFactory;
        private IActionSelector _actionSelector;
        public JsonApiRouteHandler(IActionInvokerFactory actionInvokerFactory,
            IActionSelector actionSelector)
            : this(actionInvokerFactory, actionSelector, actionContextAccessor: null)
        {
        }

        public JsonApiRouteHandler(
            IActionInvokerFactory actionInvokerFactory,
            IActionSelector actionSelector,
            IActionContextAccessor actionContextAccessor)
        {
            // The IActionContextAccessor is optional. We want to avoid the overhead of using CallContext
            // if possible.
            _actionContextAccessor = actionContextAccessor;

            _actionInvokerFactory = actionInvokerFactory;
            _actionSelector = actionSelector;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // We return null here because we're not responsible for generating the url, the route is.
            return null;
        }

        public Task RouteAsync(RouteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var candidates = _actionSelector.SelectCandidates(context);
            if (candidates == null || candidates.Count == 0)
            {
                return TaskCache.CompletedTask;
            }

            var actionDescriptor = _actionSelector.SelectBestCandidate(context, candidates);
            if (actionDescriptor == null)
            {
                return TaskCache.CompletedTask;
            }

            context.Handler = (c) =>
            {
                var routeData = c.GetRouteData();

                foreach(var routeValue in routeData.Values)
                    routeData.Values[routeValue.Key] = routeValue.Value.ToString().ToProperCase();
                    
                var actionContext = new ActionContext(context.HttpContext, routeData, actionDescriptor);
                if (_actionContextAccessor != null)
                {
                    _actionContextAccessor.ActionContext = actionContext;
                }

                var invoker = _actionInvokerFactory.CreateInvoker(actionContext);
                
                return invoker.InvokeAsync();
            };

            return TaskCache.CompletedTask;
        }
    }
}
