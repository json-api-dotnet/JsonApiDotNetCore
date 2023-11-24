using System.Reflection;
using AutoBogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// Workaround for Resharper bug at https://youtrack.jetbrains.com/issue/RSRP-494909/Breaking-UsedImplicitly-and-PublicAPI-on-types-no-longer-respected.
// ReSharper disable PropertyCanBeMadeInitOnly.Local

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
        return GetDeterministicFaker<TTarget>();
    }

    private static AutoFaker<TTarget> GetDeterministicFaker<TTarget>()
        where TTarget : class
    {
        var autoFaker = new AutoFaker<TTarget>();
        autoFaker.UseSeed(FakerContainer.GetFakerSeed());
        return autoFaker;
    }

    public AutoFaker<TTarget> CreateForObjectWithResourceId<TTarget, TId>()
        where TTarget : class
    {
        return GetDeterministicFaker<TTarget>().Configure(builder => builder.WithOverride(new ResourceStringIdOverride<TId>()));
    }

    private sealed class ResourceStringIdOverride<TId> : AutoGeneratorOverride
    {
        // AutoFaker has a class constraint, while TId has not, so we need to wrap it.
        private readonly AutoFaker<ObjectContainer<TId>> _idContainerFaker = GetDeterministicFaker<ObjectContainer<TId>>();

        public override bool CanOverride(AutoGenerateContext context)
        {
            PropertyInfo? resourceIdPropertyInfo = context.GenerateType.GetProperty("Id");
            return resourceIdPropertyInfo != null && resourceIdPropertyInfo.PropertyType == typeof(string);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            object idValue = _idContainerFaker.Generate().Value!;
            idValue = ToPositiveValue(idValue);

            ((dynamic)context.Instance).Id = idValue.ToString()!;
        }

        private static object ToPositiveValue(object idValue)
        {
            return idValue switch
            {
                short shortValue => Math.Abs(shortValue),
                int intValue => Math.Abs(intValue),
                long longValue => Math.Abs(longValue),
                _ => idValue
            };
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class ObjectContainer<TValue>
        {
            public TValue? Value { get; set; }
        }
    }
}
