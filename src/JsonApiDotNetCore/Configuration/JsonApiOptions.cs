using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Configuration
{
    public class JsonApiOptions
    {
        public string Namespace { get; set; }
        public int DefaultPageSize { get; set; }
        public bool IncludeTotalRecordCount { get; set; }
        public bool AllowClientGeneratedIds { get; set; }
        public IContextGraph ContextGraph  { get; set; }

        public void BuildContextGraph<TContext>(Action<IContextGraphBuilder> builder)
        where TContext : DbContext
        {
            var contextGraphBuilder = new ContextGraphBuilder();
            
            contextGraphBuilder.AddDbContext<TContext>();

            if(builder != null)
                builder(contextGraphBuilder);

            ContextGraph = contextGraphBuilder.Build();
        }

        public void BuildContextGraph(Action<IContextGraphBuilder> builder)
        {
            if(builder == null)
                throw new ArgumentException("Cannot build non-EF context graph without a IContextGraphBuilder action", nameof(builder));

            var contextGraphBuilder = new ContextGraphBuilder();

            builder(contextGraphBuilder);

            ContextGraph = contextGraphBuilder.Build();
        }
    }
}
