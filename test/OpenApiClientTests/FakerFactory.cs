using System.Reflection;
using AutoBogus;

namespace OpenApiClientTests;

internal sealed class FakerFactory
{
    public static FakerFactory Instance { get; } = new();

    private FakerFactory()
    {
    }

    public AutoFaker<TTarget> Create<TTarget>()
        where TTarget : class
    {
        return new AutoFaker<TTarget>();
    }

    public AutoFaker<TTarget> CreateForObjectWithResourceId<TTarget, TId>()
        where TTarget : class
    {
        return new AutoFaker<TTarget>().Configure(builder => builder.WithOverride(new ResourceStringIdOverride<TId>()));
    }

    private sealed class ResourceStringIdOverride<TId> : AutoGeneratorOverride
    {
        private readonly IAutoFaker _idFaker = AutoFaker.Create();

        public override bool CanOverride(AutoGenerateContext context)
        {
            PropertyInfo? resourceIdPropertyInfo = context.GenerateType.GetProperty("Id");
            return resourceIdPropertyInfo != null && resourceIdPropertyInfo.PropertyType == typeof(string);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            ((dynamic)context.Instance).Id = _idFaker.Generate<TId>()!.ToString()!;
        }
    }
}
