namespace JsonApiDotNetCore.Resources.Annotations;

[Flags]
public enum LinkTypes
{
    NotConfigured = 0,
    None = 1 << 0,
    Self = 1 << 1,
    Related = 1 << 2,
    DescribedBy = 1 << 3,
    Pagination = 1 << 4,
    All = Self | Related | DescribedBy | Pagination
}
