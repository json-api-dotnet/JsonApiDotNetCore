using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <summary>
    /// Contains shared assertions for derived types.
    /// </summary>
    public abstract class BaseAdapter
    {
        [AssertionMethod]
        protected static void AssertHasData<T>(SingleOrManyData<T> data, RequestAdapterState state)
            where T : class, IResourceIdentity, new()
        {
            if (!data.IsAssigned)
            {
                throw new ModelConversionException(state.Position, "The 'data' element is required.", null);
            }
        }

        [AssertionMethod]
        protected static void AssertDataHasSingleValue<T>(SingleOrManyData<T> data, bool allowNull, RequestAdapterState state)
            where T : class, IResourceIdentity, new()
        {
            if (data.SingleValue == null)
            {
                if (!allowNull)
                {
                    if (data.ManyValue == null)
                    {
                        AssertObjectIsNotNull(data.SingleValue, state);
                    }

                    throw new ModelConversionException(state.Position, "Expected an object, instead of an array.", null);
                }

                if (data.ManyValue != null)
                {
                    throw new ModelConversionException(state.Position, "Expected an object or 'null', instead of an array.", null);
                }
            }
        }

        [AssertionMethod]
        protected static void AssertDataHasManyValue<T>(SingleOrManyData<T> data, RequestAdapterState state)
            where T : class, IResourceIdentity, new()
        {
            if (data.ManyValue == null)
            {
                throw new ModelConversionException(state.Position,
                    data.SingleValue == null ? "Expected an array, instead of 'null'." : "Expected an array, instead of an object.", null);
            }
        }

        protected static void AssertObjectIsNotNull<T>([SysNotNull] T? value, RequestAdapterState state)
            where T : class
        {
            if (value is null)
            {
                throw new ModelConversionException(state.Position, "Expected an object, instead of 'null'.", null);
            }
        }
    }
}
