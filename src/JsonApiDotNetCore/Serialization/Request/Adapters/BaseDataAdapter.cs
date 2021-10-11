using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <summary>
    /// Contains shared assertions for derived types.
    /// </summary>
    public abstract class BaseDataAdapter
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
        protected static void AssertHasSingleValue<T>(SingleOrManyData<T> data, bool allowNull, RequestAdapterState state)
            where T : class, IResourceIdentity, new()
        {
            if (data.SingleValue == null)
            {
                if (!allowNull)
                {
                    throw new ModelConversionException(state.Position,
                        data.ManyValue == null
                            ? "Expected an object in 'data' element, instead of 'null'."
                            : "Expected an object in 'data' element, instead of an array.", null);
                }

                if (data.ManyValue != null)
                {
                    throw new ModelConversionException(state.Position, "Expected an object or 'null' in 'data' element, instead of an array.", null);
                }
            }
        }

        [AssertionMethod]
        protected static void AssertHasManyValue<T>(SingleOrManyData<T> data, RequestAdapterState state)
            where T : class, IResourceIdentity, new()
        {
            if (data.ManyValue == null)
            {
                throw new ModelConversionException(state.Position,
                    data.SingleValue == null
                        ? "Expected an array in 'data' element, instead of 'null'."
                        : "Expected an array in 'data' element, instead of an object.", null);
            }
        }
    }
}
