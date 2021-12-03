using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState
{
    public sealed class ModelStateValidationTests : IClassFixture<IntegrationTestContext<TestableStartup<ModelStateDbContext>, ModelStateDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ModelStateDbContext>, ModelStateDbContext> _testContext;
        private readonly ModelStateFakers _fakers = new();

        public ModelStateValidationTests(IntegrationTestContext<TestableStartup<ModelStateDbContext>, ModelStateDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<SystemDirectoriesController>();
            testContext.UseController<SystemFilesController>();
        }

        [Fact]
        public async Task Cannot_create_resource_with_omitted_required_attribute()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    attributes = new
                    {
                        isCaseSensitive = true
                    }
                }
            };

            const string route = "/systemDirectories";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Input validation failed.");
            error.Detail.Should().Be("The Name field is required.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/data/attributes/directoryName");
        }

        [Fact]
        public async Task Cannot_create_resource_with_null_for_required_attribute_value()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    attributes = new
                    {
                        directoryName = (string?)null,
                        isCaseSensitive = true
                    }
                }
            };

            const string route = "/systemDirectories";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Input validation failed.");
            error.Detail.Should().Be("The Name field is required.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/data/attributes/directoryName");
        }

        [Fact]
        public async Task Cannot_create_resource_with_invalid_attribute_value()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    attributes = new
                    {
                        directoryName = "!@#$%^&*().-",
                        isCaseSensitive = true
                    }
                }
            };

            const string route = "/systemDirectories";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Input validation failed.");
            error.Detail.Should().Be("The field Name must match the regular expression '^[\\w\\s]+$'.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/data/attributes/directoryName");
        }

        [Fact]
        public async Task Can_create_resource_with_valid_attribute_value()
        {
            // Arrange
            SystemDirectory newDirectory = _fakers.SystemDirectory.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    attributes = new
                    {
                        directoryName = newDirectory.Name,
                        isCaseSensitive = newDirectory.IsCaseSensitive
                    }
                }
            };

            const string route = "/systemDirectories";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("directoryName").With(value => value.Should().Be(newDirectory.Name));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("isCaseSensitive").With(value => value.Should().Be(newDirectory.IsCaseSensitive));
        }

        [Fact]
        public async Task Cannot_create_resource_with_multiple_violations()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    attributes = new
                    {
                        isCaseSensitive = false,
                        sizeInBytes = -1
                    }
                }
            };

            const string route = "/systemDirectories";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(2);

            ErrorObject error1 = responseDocument.Errors[0];
            error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error1.Title.Should().Be("Input validation failed.");
            error1.Detail.Should().Be("The Name field is required.");
            error1.Source.ShouldNotBeNull();
            error1.Source.Pointer.Should().Be("/data/attributes/directoryName");

            ErrorObject error2 = responseDocument.Errors[1];
            error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error2.Title.Should().Be("Input validation failed.");
            error2.Detail.Should().Be("The field SizeInBytes must be between 0 and 9223372036854775807.");
            error2.Source.ShouldNotBeNull();
            error2.Source.Pointer.Should().Be("/data/attributes/sizeInBytes");
        }

        [Fact]
        public async Task Does_not_exceed_MaxModelValidationErrors()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    attributes = new
                    {
                        sizeInBytes = -1
                    }
                }
            };

            const string route = "/systemDirectories";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(3);

            ErrorObject error1 = responseDocument.Errors[0];
            error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error1.Title.Should().Be("Input validation failed.");
            error1.Detail.Should().Be("The maximum number of allowed model errors has been reached.");
            error1.Source.Should().BeNull();

            ErrorObject error2 = responseDocument.Errors[1];
            error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error2.Title.Should().Be("Input validation failed.");
            error2.Detail.Should().Be("The Name field is required.");
            error2.Source.ShouldNotBeNull();
            error2.Source.Pointer.Should().Be("/data/attributes/directoryName");

            ErrorObject error3 = responseDocument.Errors[2];
            error3.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error3.Title.Should().Be("Input validation failed.");
            error3.Detail.Should().Be("The IsCaseSensitive field is required.");
            error3.Source.ShouldNotBeNull();
            error3.Source.Pointer.Should().Be("/data/attributes/isCaseSensitive");
        }

        [Fact]
        public async Task Can_create_resource_with_annotated_relationships()
        {
            // Arrange
            SystemDirectory existingParentDirectory = _fakers.SystemDirectory.Generate();
            SystemDirectory existingSubdirectory = _fakers.SystemDirectory.Generate();
            SystemFile existingFile = _fakers.SystemFile.Generate();

            SystemDirectory newDirectory = _fakers.SystemDirectory.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.AddRange(existingParentDirectory, existingSubdirectory);
                dbContext.Files.Add(existingFile);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    attributes = new
                    {
                        directoryName = newDirectory.Name,
                        isCaseSensitive = newDirectory.IsCaseSensitive
                    },
                    relationships = new
                    {
                        subdirectories = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "systemDirectories",
                                    id = existingSubdirectory.StringId
                                }
                            }
                        },
                        files = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "systemFiles",
                                    id = existingFile.StringId
                                }
                            }
                        },
                        parent = new
                        {
                            data = new
                            {
                                type = "systemDirectories",
                                id = existingParentDirectory.StringId
                            }
                        }
                    }
                }
            };

            const string route = "/systemDirectories";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("directoryName").With(value => value.Should().Be(newDirectory.Name));
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("isCaseSensitive").With(value => value.Should().Be(newDirectory.IsCaseSensitive));
        }

        [Fact]
        public async Task Can_add_to_annotated_ToMany_relationship()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();
            SystemFile existingFile = _fakers.SystemFile.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingDirectory, existingFile);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "systemFiles",
                        id = existingFile.StringId
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}/relationships/files";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_update_resource_with_omitted_required_attribute_value()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();

            long newSizeInBytes = _fakers.SystemDirectory.Generate().SizeInBytes;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.Add(existingDirectory);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = existingDirectory.StringId,
                    attributes = new
                    {
                        sizeInBytes = newSizeInBytes
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_update_resource_with_null_for_required_attribute_values()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.Add(existingDirectory);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = existingDirectory.StringId,
                    attributes = new
                    {
                        directoryName = (string?)null,
                        isCaseSensitive = (bool?)null
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(2);

            ErrorObject error1 = responseDocument.Errors[0];
            error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error1.Title.Should().Be("Input validation failed.");
            error1.Detail.Should().Be("The Name field is required.");
            error1.Source.ShouldNotBeNull();
            error1.Source.Pointer.Should().Be("/data/attributes/directoryName");

            ErrorObject error2 = responseDocument.Errors[1];
            error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error2.Title.Should().Be("Input validation failed.");
            error2.Detail.Should().Be("The IsCaseSensitive field is required.");
            error2.Source.ShouldNotBeNull();
            error2.Source.Pointer.Should().Be("/data/attributes/isCaseSensitive");
        }

        [Fact]
        public async Task Cannot_update_resource_with_invalid_attribute_value()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.Add(existingDirectory);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = existingDirectory.StringId,
                    attributes = new
                    {
                        directoryName = "!@#$%^&*().-"
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Input validation failed.");
            error.Detail.Should().Be("The field Name must match the regular expression '^[\\w\\s]+$'.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/data/attributes/directoryName");
        }

        [Fact]
        public async Task Cannot_update_resource_with_invalid_ID()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = "-1",
                    relationships = new
                    {
                        subdirectories = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "systemDirectories",
                                    id = "-1"
                                }
                            }
                        }
                    }
                }
            };

            const string route = "/systemDirectories/-1";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(2);

            ErrorObject error1 = responseDocument.Errors[0];
            error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error1.Title.Should().Be("Input validation failed.");
            error1.Detail.Should().Be("The field Id must match the regular expression '^[0-9]+$'.");
            error1.Source.ShouldNotBeNull();
            error1.Source.Pointer.Should().Be("/data/id");

            ErrorObject error2 = responseDocument.Errors[1];
            error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error2.Title.Should().Be("Input validation failed.");
            error2.Detail.Should().Be("The field Id must match the regular expression '^[0-9]+$'.");
            error2.Source.ShouldNotBeNull();
            error2.Source.Pointer.Should().Be("/data/relationships/subdirectories/data[0]/id");
        }

        [Fact]
        public async Task Can_update_resource_with_valid_attribute_value()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();

            string newDirectoryName = _fakers.SystemDirectory.Generate().Name;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.Add(existingDirectory);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = existingDirectory.StringId,
                    attributes = new
                    {
                        directoryName = newDirectoryName
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_update_resource_with_annotated_relationships()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();
            existingDirectory.Subdirectories = _fakers.SystemDirectory.Generate(1);
            existingDirectory.Files = _fakers.SystemFile.Generate(1);
            existingDirectory.Parent = _fakers.SystemDirectory.Generate();

            SystemDirectory existingParent = _fakers.SystemDirectory.Generate();
            SystemDirectory existingSubdirectory = _fakers.SystemDirectory.Generate();
            SystemFile existingFile = _fakers.SystemFile.Generate();

            string newDirectoryName = _fakers.SystemDirectory.Generate().Name;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.AddRange(existingDirectory, existingParent, existingSubdirectory);
                dbContext.Files.Add(existingFile);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = existingDirectory.StringId,
                    attributes = new
                    {
                        directoryName = newDirectoryName
                    },
                    relationships = new
                    {
                        subdirectories = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "systemDirectories",
                                    id = existingSubdirectory.StringId
                                }
                            }
                        },
                        files = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "systemFiles",
                                    id = existingFile.StringId
                                }
                            }
                        },
                        parent = new
                        {
                            data = new
                            {
                                type = "systemDirectories",
                                id = existingParent.StringId
                            }
                        }
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_update_resource_with_multiple_self_references()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.Add(existingDirectory);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = existingDirectory.StringId,
                    relationships = new
                    {
                        self = new
                        {
                            data = new
                            {
                                type = "systemDirectories",
                                id = existingDirectory.StringId
                            }
                        },
                        alsoSelf = new
                        {
                            data = new
                            {
                                type = "systemDirectories",
                                id = existingDirectory.StringId
                            }
                        }
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_update_resource_with_collection_of_self_references()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.Add(existingDirectory);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = existingDirectory.StringId,
                    relationships = new
                    {
                        subdirectories = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "systemDirectories",
                                    id = existingDirectory.StringId
                                }
                            }
                        }
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_replace_annotated_ToOne_relationship()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();
            existingDirectory.Parent = _fakers.SystemDirectory.Generate();

            SystemDirectory otherExistingDirectory = _fakers.SystemDirectory.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.AddRange(existingDirectory, otherExistingDirectory);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = otherExistingDirectory.StringId
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}/relationships/parent";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_replace_annotated_ToMany_relationship()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();
            existingDirectory.Files = _fakers.SystemFile.Generate(2);

            SystemFile existingFile = _fakers.SystemFile.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddInRange(existingDirectory, existingFile);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "systemFiles",
                        id = existingFile.StringId
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}/relationships/files";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_remove_from_annotated_ToMany_relationship()
        {
            // Arrange
            SystemDirectory existingDirectory = _fakers.SystemDirectory.Generate();
            existingDirectory.Files = _fakers.SystemFile.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.Add(existingDirectory);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "systemFiles",
                        id = existingDirectory.Files.ElementAt(0).StringId
                    }
                }
            };

            string route = $"/systemDirectories/{existingDirectory.StringId}/relationships/files";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }
    }
}
