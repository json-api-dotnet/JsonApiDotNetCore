using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding
{
    [PublicAPI]
    public sealed class LambdaParameterNameScope : IDisposable
    {
        private readonly LambdaParameterNameFactory _owner;

        public string Name { get; }

        public LambdaParameterNameScope(string name, LambdaParameterNameFactory owner)
        {
            ArgumentGuard.NotNullNorEmpty(name, nameof(name));
            ArgumentGuard.NotNull(owner, nameof(owner));

            Name = name;
            _owner = owner;
        }

        public void Dispose()
        {
            _owner.Release(Name);
        }
    }
}
