using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Compares `IIdentifiable` instances with each other based on their type and <see cref="IIdentifiable.StringId" />, falling back to
    /// <see cref="IIdentifiable.LocalId" /> when both StringIds are null.
    /// </summary>
    [PublicAPI]
    public sealed class IdentifiableComparer : IEqualityComparer<IIdentifiable>
    {
        public static readonly IdentifiableComparer Instance = new();

        private IdentifiableComparer()
        {
        }

        public bool Equals(IIdentifiable? left, IIdentifiable? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null || left.GetType() != right.GetType())
            {
                return false;
            }

            if (left.StringId == null && right.StringId == null)
            {
                return left.LocalId == right.LocalId;
            }

            return left.StringId == right.StringId;
        }

        public int GetHashCode(IIdentifiable obj)
        {
            // LocalId is intentionally omitted here, it is okay for hashes to collide.
            return HashCode.Combine(obj.GetType(), obj.StringId);
        }
    }
}
