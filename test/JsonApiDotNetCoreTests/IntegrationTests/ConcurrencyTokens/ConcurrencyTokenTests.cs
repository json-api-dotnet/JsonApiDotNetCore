using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ConcurrencyTokens
{
    public sealed class ConcurrencyTokenTests : IClassFixture<IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> _testContext;
        private readonly ConcurrencyFakers _fakers = new();

        public ConcurrencyTokenTests(IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<PartitionsController>();
            testContext.UseController<DisksController>();
        }

        [Fact]
        public async Task Can_get_primary_resource_by_ID_with_include()
        {
            // Arrange
            Disk disk = _fakers.Disk.Generate();
            disk.Partitions = _fakers.Partition.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Disks.Add(disk);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/disks/{disk.StringId}?include=partitions";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("disks");
            responseDocument.Data.SingleValue.Id.Should().Be(disk.StringId);
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("manufacturer").With(value => value.Should().Be(disk.Manufacturer));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("concurrencyToken").With(value => value.Should().Be(disk.xmin));
            responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

            responseDocument.Included.ShouldHaveCount(1);
            responseDocument.Included[0].Type.Should().Be("partitions");
            responseDocument.Included[0].Id.Should().Be(disk.Partitions[0].StringId);
            responseDocument.Included[0].Attributes.ShouldContainKey("mountPoint").With(value => value.Should().Be(disk.Partitions[0].MountPoint));
            responseDocument.Included[0].Attributes.ShouldContainKey("fileSystem").With(value => value.Should().Be(disk.Partitions[0].FileSystem));
            responseDocument.Included[0].Attributes.ShouldContainKey("capacityInBytes").With(value => value.Should().Be(disk.Partitions[0].CapacityInBytes));
            responseDocument.Included[0].Attributes.ShouldContainKey("freeSpaceInBytes").With(value => value.Should().Be(disk.Partitions[0].FreeSpaceInBytes));
            responseDocument.Included[0].Attributes.ShouldContainKey("concurrencyToken").With(value => value.Should().Be(disk.Partitions[0].xmin));
            responseDocument.Included[0].Relationships.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task Can_create_resource()
        {
            // Arrange
            string newManufacturer = _fakers.Disk.Generate().Manufacturer;
            string newSerialCode = _fakers.Disk.Generate().SerialCode;

            var requestBody = new
            {
                data = new
                {
                    type = "disks",
                    attributes = new
                    {
                        manufacturer = newManufacturer,
                        serialCode = newSerialCode
                    }
                }
            };

            const string route = "/disks";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("disks");
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("manufacturer").With(value => value.Should().Be(newManufacturer));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("serialCode").With(value => value.Should().Be(newSerialCode));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("concurrencyToken").With(value => value.As<uint>().Should().BeGreaterThan(0));
            responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

            long newDiskId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Disk diskInDatabase = await dbContext.Disks.FirstWithIdAsync(newDiskId);

                diskInDatabase.Manufacturer.Should().Be(newManufacturer);
                diskInDatabase.SerialCode.Should().Be(newSerialCode);
                diskInDatabase.xmin.Should().BeGreaterThan(0);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_ignored_token()
        {
            // Arrange
            string newManufacturer = _fakers.Disk.Generate().Manufacturer;
            string newSerialCode = _fakers.Disk.Generate().SerialCode;
            const uint ignoredToken = 98765432;

            var requestBody = new
            {
                data = new
                {
                    type = "disks",
                    attributes = new
                    {
                        manufacturer = newManufacturer,
                        serialCode = newSerialCode,
                        concurrencyToken = ignoredToken
                    }
                }
            };

            const string route = "/disks";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("disks");
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("manufacturer").With(value => value.Should().Be(newManufacturer));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("serialCode").With(value => value.Should().Be(newSerialCode));

            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("concurrencyToken").With(value =>
            {
                long typedValue = value.As<uint>();
                typedValue.Should().BeGreaterThan(0);
                typedValue.Should().NotBe(ignoredToken);
            });

            responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

            long newDiskId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Disk diskInDatabase = await dbContext.Disks.FirstWithIdAsync(newDiskId);

                diskInDatabase.Manufacturer.Should().Be(newManufacturer);
                diskInDatabase.SerialCode.Should().Be(newSerialCode);
                diskInDatabase.xmin.Should().BeGreaterThan(0).And.NotBe(ignoredToken);
            });
        }

        [Fact(Skip = "There is no way to send the token, which is needed to find the related resource.")]
        public async Task Can_create_resource_with_relationship()
        {
            // Arrange
            Partition existingPartition = _fakers.Partition.Generate();

            string newManufacturer = _fakers.Disk.Generate().Manufacturer;
            string newSerialCode = _fakers.Disk.Generate().SerialCode;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Partitions.Add(existingPartition);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "disks",
                    attributes = new
                    {
                        manufacturer = newManufacturer,
                        serialCode = newSerialCode
                    },
                    relationships = new
                    {
                        partitions = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "partitions",
                                    id = existingPartition.StringId
                                }
                            }
                        }
                    }
                }
            };

            const string route = "/disks";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("disks");
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("manufacturer").With(value => value.Should().Be(newManufacturer));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("serialCode").With(value => value.Should().Be(newSerialCode));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("concurrencyToken").With(value => value.As<uint>().Should().BeGreaterThan(0));
            responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

            long newDiskId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Disk diskInDatabase = await dbContext.Disks.Include(disk => disk.Partitions).FirstWithIdAsync(newDiskId);

                diskInDatabase.Manufacturer.Should().Be(newManufacturer);
                diskInDatabase.SerialCode.Should().Be(newSerialCode);
                diskInDatabase.xmin.Should().BeGreaterThan(0);

                diskInDatabase.Partitions.ShouldHaveCount(1);
                diskInDatabase.Partitions[0].Id.Should().Be(existingPartition.Id);
            });
        }

        [Fact]
        public async Task Can_update_resource_using_fresh_token()
        {
            // Arrange
            Disk existingDisk = _fakers.Disk.Generate();

            string newSerialCode = _fakers.Disk.Generate().SerialCode;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Disks.Add(existingDisk);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "disks",
                    id = existingDisk.StringId,
                    attributes = new
                    {
                        serialCode = newSerialCode,
                        concurrencyToken = existingDisk.xmin
                    }
                }
            };

            string route = "/disks/" + existingDisk.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("disks");
            responseDocument.Data.SingleValue.Id.Should().Be(existingDisk.StringId);
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("manufacturer").With(value => value.Should().Be(existingDisk.Manufacturer));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("serialCode").With(value => value.Should().Be(newSerialCode));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("concurrencyToken").With(value => value.Should().NotBe(existingDisk.xmin));
            responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Disk diskInDatabase = await dbContext.Disks.FirstWithIdAsync(existingDisk.Id);

                diskInDatabase.Manufacturer.Should().Be(existingDisk.Manufacturer);
                diskInDatabase.SerialCode.Should().Be(newSerialCode);
                diskInDatabase.xmin.Should().NotBe(existingDisk.xmin);
            });
        }

        [Fact]
        public async Task Cannot_update_resource_using_stale_token()
        {
            // Arrange
            Disk existingDisk = _fakers.Disk.Generate();

            string newSerialCode = _fakers.Disk.Generate().SerialCode;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Disks.Add(existingDisk);
                await dbContext.SaveChangesAsync();
                await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"Disks\" set \"Manufacturer\" = 'other' where \"Id\" = {existingDisk.Id}");
            });

            var requestBody = new
            {
                data = new
                {
                    type = "disks",
                    id = existingDisk.StringId,
                    attributes = new
                    {
                        serialCode = newSerialCode,
                        concurrencyToken = existingDisk.xmin
                    }
                }
            };

            string route = "/disks/" + existingDisk.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Conflict);
            error.Title.Should().StartWith("The concurrency token is missing or does not match the server version.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_update_resource_without_token()
        {
            // Arrange
            Disk existingDisk = _fakers.Disk.Generate();

            string newSerialCode = _fakers.Disk.Generate().SerialCode;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Disks.Add(existingDisk);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "disks",
                    id = existingDisk.StringId,
                    attributes = new
                    {
                        serialCode = newSerialCode
                    }
                }
            };

            string route = "/disks/" + existingDisk.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Conflict);
            error.Title.Should().StartWith("The concurrency token is missing or does not match the server version.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Can_update_resource_with_HasOne_relationship()
        {
            // Arrange
            Partition existingPartition = _fakers.Partition.Generate();
            existingPartition.Owner = _fakers.Disk.Generate();

            Disk existingDisk = _fakers.Disk.Generate();

            ulong? newFreeSpaceInBytes = _fakers.Partition.Generate().FreeSpaceInBytes;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingPartition, existingDisk);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "partitions",
                    id = existingPartition.StringId,
                    attributes = new
                    {
                        freeSpaceInBytes = newFreeSpaceInBytes,
                        concurrencyToken = existingPartition.xmin
                    },
                    relationships = new
                    {
                        owner = new
                        {
                            data = new
                            {
                                type = "disks",
                                id = existingDisk.StringId
                            }
                        }
                    }
                }
            };

            string route = "/partitions/" + existingPartition.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            ulong? capacityInBytes = existingPartition.CapacityInBytes;

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("partitions");
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("capacityInBytes").With(value => value.Should().Be(capacityInBytes));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("freeSpaceInBytes").With(value => value.Should().Be(newFreeSpaceInBytes));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("concurrencyToken").With(value => value.As<uint>().Should().BeGreaterThan(0));
            responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Partition partitionInDatabase = await dbContext.Partitions.Include(partition => partition.Owner).FirstWithIdAsync(existingPartition.Id);

                partitionInDatabase.CapacityInBytes.Should().Be(capacityInBytes);
                partitionInDatabase.FreeSpaceInBytes.Should().Be(newFreeSpaceInBytes);
                partitionInDatabase.xmin.Should().BeGreaterThan(0);

                partitionInDatabase.Owner.ShouldNotBeNull();
                partitionInDatabase.Owner.Id.Should().Be(existingDisk.Id);
            });
        }

        [Fact(Skip = "There is no way to send the token, which is needed to find the related resource.")]
        public async Task Can_update_resource_with_HasMany_relationship()
        {
            // Arrange
            Disk existingDisk = _fakers.Disk.Generate();
            existingDisk.Partitions = _fakers.Partition.Generate(1);

            Partition existingPartition = _fakers.Partition.Generate();

            string newSerialCode = _fakers.Disk.Generate().SerialCode;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingDisk, existingPartition);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "disks",
                    id = existingDisk.StringId,
                    attributes = new
                    {
                        serialCode = newSerialCode,
                        concurrencyToken = existingDisk.xmin
                    },
                    relationships = new
                    {
                        partitions = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "partitions",
                                    id = existingPartition.StringId
                                }
                            }
                        }
                    }
                }
            };

            string route = "/disks/" + existingDisk.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("disks");
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("manufacturer").With(value => value.Should().Be(existingDisk.Manufacturer));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("serialCode").With(value => value.Should().Be(newSerialCode));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("concurrencyToken").With(value => value.As<uint>().Should().BeGreaterThan(0));
            responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Disk diskInDatabase = await dbContext.Disks.Include(disk => disk.Partitions).FirstWithIdAsync(existingDisk.Id);

                diskInDatabase.Manufacturer.Should().Be(existingDisk.Manufacturer);
                diskInDatabase.SerialCode.Should().Be(newSerialCode);
                diskInDatabase.xmin.Should().BeGreaterThan(0);

                diskInDatabase.Partitions.ShouldHaveCount(1);
                diskInDatabase.Partitions[0].Id.Should().Be(existingPartition.Id);
            });
        }

        [Fact(Skip = "There is no way to send the token, which is needed to find the resource.")]
        public async Task Can_delete_resource()
        {
            // Arrange
            Disk existingDisk = _fakers.Disk.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Disks.Add(existingDisk);
                await dbContext.SaveChangesAsync();
            });

            string route = "/disks/" + existingDisk.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Disk diskInDatabase = await dbContext.Disks.FirstOrDefaultAsync(disk => disk.Id == existingDisk.Id);

                diskInDatabase.Should().BeNull();
            });
        }

        [Fact(Skip = "There is no way to send the token, which is needed to find the related resource.")]
        public async Task Can_add_to_HasMany_relationship()
        {
            // Arrange
            Disk existingDisk = _fakers.Disk.Generate();
            Partition existingPartition = _fakers.Partition.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingDisk, existingPartition);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "partitions",
                        id = existingPartition.StringId
                    }
                }
            };

            string route = $"/disks/{existingDisk.StringId}/relationships/partitions";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Disk diskInDatabase = await dbContext.Disks.Include(disk => disk.Partitions).FirstWithIdAsync(existingDisk.Id);

                diskInDatabase.Partitions.ShouldHaveCount(1);
                diskInDatabase.Partitions[0].Id.Should().Be(existingPartition.Id);
            });
        }

        [Fact(Skip = "There is no way to send the token, which is needed to find the related resource.")]
        public async Task Can_remove_from_HasMany_relationship()
        {
            // Arrange
            Disk existingDisk = _fakers.Disk.Generate();
            existingDisk.Partitions = _fakers.Partition.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Disks.Add(existingDisk);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "partitions",
                        id = existingDisk.Partitions[1].StringId
                    }
                }
            };

            string route = $"/disks/{existingDisk.StringId}/relationships/partitions";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Disk diskInDatabase = await dbContext.Disks.Include(disk => disk.Partitions).FirstWithIdAsync(existingDisk.Id);

                diskInDatabase.Partitions.ShouldHaveCount(1);
                diskInDatabase.Partitions[0].Id.Should().Be(existingDisk.Partitions[0].Id);
            });
        }
    }
}
