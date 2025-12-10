using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

public sealed class ResourceDefinitionSerializationTests
    : IClassFixture<IntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext> _testContext;
    private readonly SerializationFakers _fakers = new();

    public ResourceDefinitionSerializationTests(IntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<StudentsController>();
        testContext.UseController<ScholarshipsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceDefinition<StudentDefinition>();

            services.AddSingleton<IEncryptionService, AesEncryptionService>();
            services.AddSingleton<ResourceDefinitionHitCounter>();
            services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
        });

        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();
        hitCounter.Reset();
    }

    [Fact]
    public async Task Encrypts_on_get_primary_resources()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Student> students = _fakers.Student.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Student>();
            dbContext.Students.AddRange(students);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/students";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(students[0].SocialSecurityNumber);
        });

        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(students[1].SocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize),
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Encrypts_on_get_primary_resources_with_ToMany_include()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Scholarship> scholarships = _fakers.Scholarship.GenerateList(2);
        scholarships[0].Participants = _fakers.Student.GenerateList(2);
        scholarships[1].Participants = _fakers.Student.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Scholarship>();
            dbContext.Scholarships.AddRange(scholarships);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/scholarships?include=participants";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Included.Should().HaveCount(4);

        responseDocument.Included[0].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(scholarships[0].Participants[0].SocialSecurityNumber);
        });

        responseDocument.Included[1].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(scholarships[0].Participants[1].SocialSecurityNumber);
        });

        responseDocument.Included[2].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(scholarships[1].Participants[0].SocialSecurityNumber);
        });

        responseDocument.Included[3].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(scholarships[1].Participants[1].SocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize),
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize),
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize),
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Encrypts_on_get_primary_resource_by_ID()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Student student = _fakers.Student.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Students.Add(student);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/students/{student.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(student.SocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Encrypts_on_get_secondary_resources()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship scholarship = _fakers.Scholarship.GenerateOne();
        scholarship.Participants = _fakers.Student.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Scholarships.Add(scholarship);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/scholarships/{scholarship.StringId}/participants";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(scholarship.Participants[0].SocialSecurityNumber);
        });

        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(scholarship.Participants[1].SocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize),
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Encrypts_on_get_secondary_resource()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship scholarship = _fakers.Scholarship.GenerateOne();
        scholarship.PrimaryContact = _fakers.Student.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Scholarships.Add(scholarship);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/scholarships/{scholarship.StringId}/primaryContact";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(scholarship.PrimaryContact.SocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Encrypts_on_get_secondary_resource_with_ToOne_include()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship scholarship = _fakers.Scholarship.GenerateOne();
        scholarship.PrimaryContact = _fakers.Student.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Scholarships.Add(scholarship);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/scholarships/{scholarship.StringId}?include=primaryContact";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Included.Should().HaveCount(1);

        responseDocument.Included[0].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(scholarship.PrimaryContact.SocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Decrypts_on_create_resource()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        string newName = _fakers.Student.GenerateOne().Name;
        string newSocialSecurityNumber = _fakers.Student.GenerateOne().SocialSecurityNumber;

        var requestBody = new
        {
            data = new
            {
                type = "students",
                attributes = new
                {
                    name = newName,
                    socialSecurityNumber = encryptionService.Encrypt(newSocialSecurityNumber)
                }
            }
        };

        const string route = "/students";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(newSocialSecurityNumber);
        });

        long newStudentId = long.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Student studentInDatabase = await dbContext.Students.FirstWithIdAsync(newStudentId);

            studentInDatabase.SocialSecurityNumber.Should().Be(newSocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnDeserialize),
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Encrypts_on_create_resource_with_included_ToOne_relationship()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Student existingStudent = _fakers.Student.GenerateOne();

        string newProgramName = _fakers.Scholarship.GenerateOne().ProgramName;
        decimal newAmount = _fakers.Scholarship.GenerateOne().Amount;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Students.Add(existingStudent);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "scholarships",
                attributes = new
                {
                    programName = newProgramName,
                    amount = newAmount
                },
                relationships = new
                {
                    primaryContact = new
                    {
                        data = new
                        {
                            type = "students",
                            id = existingStudent.StringId
                        }
                    }
                }
            }
        };

        const string route = "/scholarships?include=primaryContact";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Included.Should().HaveCount(1);

        responseDocument.Included[0].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(existingStudent.SocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Decrypts_on_update_resource()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Student existingStudent = _fakers.Student.GenerateOne();

        string newSocialSecurityNumber = _fakers.Student.GenerateOne().SocialSecurityNumber;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Students.Add(existingStudent);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "students",
                id = existingStudent.StringId,
                attributes = new
                {
                    socialSecurityNumber = encryptionService.Encrypt(newSocialSecurityNumber)
                }
            }
        };

        string route = $"/students/{existingStudent.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(newSocialSecurityNumber);
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Student studentInDatabase = await dbContext.Students.FirstWithIdAsync(existingStudent.Id);

            studentInDatabase.SocialSecurityNumber.Should().Be(newSocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnDeserialize),
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Encrypts_on_update_resource_with_included_ToMany_relationship()
    {
        // Arrange
        var encryptionService = _testContext.Factory.Services.GetRequiredService<IEncryptionService>();
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship existingScholarship = _fakers.Scholarship.GenerateOne();
        existingScholarship.Participants = _fakers.Student.GenerateList(3);

        decimal newAmount = _fakers.Scholarship.GenerateOne().Amount;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Scholarships.Add(existingScholarship);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "scholarships",
                id = existingScholarship.StringId,
                attributes = new
                {
                    amount = newAmount
                },
                relationships = new
                {
                    participants = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "students",
                                id = existingScholarship.Participants[0].StringId
                            },
                            new
                            {
                                type = "students",
                                id = existingScholarship.Participants[2].StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/scholarships/{existingScholarship.StringId}?include=participants";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(existingScholarship.Participants[0].SocialSecurityNumber);
        });

        responseDocument.Included[1].Attributes.Should().ContainKey("socialSecurityNumber").WhoseValue.With(value =>
        {
            string stringValue = value.Should().BeOfType<string?>().Subject.Should().NotBeNull().And.Subject;
            string socialSecurityNumber = encryptionService.Decrypt(stringValue);

            socialSecurityNumber.Should().Be(existingScholarship.Participants[2].SocialSecurityNumber);
        });

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize),
            (typeof(Student), ResourceDefinitionExtensibilityPoints.OnSerialize)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Skips_on_get_ToOne_relationship()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship scholarship = _fakers.Scholarship.GenerateOne();
        scholarship.PrimaryContact = _fakers.Student.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Scholarships.Add(scholarship);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/scholarships/{scholarship.StringId}/relationships/primaryContact";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(scholarship.PrimaryContact.StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Skips_on_get_ToMany_relationship()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship scholarship = _fakers.Scholarship.GenerateOne();
        scholarship.Participants = _fakers.Student.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Scholarships.Add(scholarship);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/scholarships/{scholarship.StringId}/relationships/participants";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(scholarship.Participants[0].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(scholarship.Participants[1].StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Skips_on_update_ToOne_relationship()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship existingScholarship = _fakers.Scholarship.GenerateOne();
        Student existingStudent = _fakers.Student.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingScholarship, existingStudent);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "students",
                id = existingStudent.StringId
            }
        };

        string route = $"/scholarships/{existingScholarship.StringId}/relationships/primaryContact";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Skips_on_set_ToMany_relationship()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship existingScholarship = _fakers.Scholarship.GenerateOne();
        List<Student> existingStudents = _fakers.Student.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Scholarships.Add(existingScholarship);
            dbContext.Students.AddRange(existingStudents);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "students",
                    id = existingStudents[0].StringId
                },
                new
                {
                    type = "students",
                    id = existingStudents[1].StringId
                }
            }
        };

        string route = $"/scholarships/{existingScholarship.StringId}/relationships/participants";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Skips_on_add_to_ToMany_relationship()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship existingScholarship = _fakers.Scholarship.GenerateOne();
        List<Student> existingStudents = _fakers.Student.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Scholarships.Add(existingScholarship);
            dbContext.Students.AddRange(existingStudents);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "students",
                    id = existingStudents[0].StringId
                },
                new
                {
                    type = "students",
                    id = existingStudents[1].StringId
                }
            }
        };

        string route = $"/scholarships/{existingScholarship.StringId}/relationships/participants";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Skips_on_remove_from_ToMany_relationship()
    {
        // Arrange
        var hitCounter = _testContext.Factory.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Scholarship existingScholarship = _fakers.Scholarship.GenerateOne();
        existingScholarship.Participants = _fakers.Student.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Scholarships.Add(existingScholarship);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "students",
                    id = existingScholarship.Participants[0].StringId
                },
                new
                {
                    type = "students",
                    id = existingScholarship.Participants[1].StringId
                }
            }
        };

        string route = $"/scholarships/{existingScholarship.StringId}/relationships/participants";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        hitCounter.HitExtensibilityPoints.Should().BeEmpty();
    }
}
