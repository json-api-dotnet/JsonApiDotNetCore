using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdCompaction;

public class CompactGuidConverter() : ValueConverter<CompactGuid, Guid>(
    id => (Guid)id,
    value => new CompactGuid(value));
