using System;
using JsonApiDotNetCore.Resources;

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable AV1008 // Class should not be static

namespace TestBuildingBlocks
{
    public static class Unknown
    {
        public const string ResourceType = "doesNotExist1";
        public const string Relationship = "doesNotExist2";
        public const string LocalId = "doesNotExist3";

        public static class TypedId
        {
            public const short Int16 = short.MaxValue;
            public const short AltInt16 = Int16 - 1;

            public const int Int32 = int.MaxValue;
            public const int AltInt32 = Int32 - 1;

            public const long Int64 = long.MaxValue;
            public const long AltInt64 = Int64 - 1;

            public static readonly Guid Guid = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
            public static readonly Guid AltGuid = Guid.Parse("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE");
        }

        public static class StringId
        {
            public static readonly string Int16 = TypedId.Int16.ToString();
            public static readonly string AltInt16 = TypedId.AltInt16.ToString();

            public static readonly string Int32 = TypedId.Int32.ToString();
            public static readonly string AltInt32 = TypedId.AltInt32.ToString();

            public static readonly string Int64 = TypedId.Int64.ToString();
            public static readonly string AltInt64 = TypedId.AltInt64.ToString();

            public static readonly string Guid = TypedId.Guid.ToString();
            public static readonly string AltGuid = TypedId.AltGuid.ToString();

            public static string For<TResource, TId>()
                where TResource : IIdentifiable<TId>
            {
                return InnerFor<TResource, TId>(false);
            }

            public static string AltFor<TResource, TId>()
                where TResource : IIdentifiable<TId>
            {
                return InnerFor<TResource, TId>(true);
            }

            private static string InnerFor<TResource, TId>(bool isAlt)
                where TResource : IIdentifiable<TId>
            {
                Type type = typeof(TId);

                if (type == typeof(short))
                {
                    return isAlt ? AltInt16 : Int16;
                }

                if (type == typeof(int))
                {
                    return isAlt ? AltInt32 : Int32;
                }

                if (type == typeof(long))
                {
                    return isAlt ? AltInt64 : Int64;
                }

                if (type == typeof(Guid))
                {
                    return isAlt ? AltGuid : Guid;
                }

                throw new NotSupportedException(
                    $"Unsupported '{nameof(Identifiable.Id)}' property of type '{type}' on resource type '{typeof(TResource).Name}'.");
            }
        }
    }
}
