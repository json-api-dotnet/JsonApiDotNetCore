using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.UnitTests.AssemblyTypeLocator;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class InterfaceImplementation : IGenericInterface<int>;
