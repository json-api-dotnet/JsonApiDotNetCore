using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Configuration
{
    public class JsonApiOptions
    {
        public string Namespace { get; set; }
        public int DefaultPageSize { get; set; }
        public bool IncludeTotalRecordCount { get; set; }
        public bool AllowClientGeneratedIds { get; set; }
        public IContextGraph ContextGraph  { get; set; }
        public IContractResolver JsonContractResolver  { get; set; }  = new DasherizedResolver();

        public void BuildContextGraph<TContext>(Action<IContextGraphBuilder> builder)
        where TContext : DbContext
        {
            var contextGraphBuilder = new ContextGraphBuilder();
            
            contextGraphBuilder.AddDbContext<TContext>();

            builder?.Invoke(contextGraphBuilder);

            ContextGraph = contextGraphBuilder.Build();
        }

        public void BuildContextGraph(Action<IContextGraphBuilder> builder)
        {
            if(builder == null)
                throw new ArgumentException("Cannot build non-EF context graph without an IContextGraphBuilder action", nameof(builder));

            var contextGraphBuilder = new ContextGraphBuilder();

            builder(contextGraphBuilder);

            ContextGraph = contextGraphBuilder.Build();
        }
    }
}
