using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class ModelStateValidationTests : IClassFixture<IntegrationTestContext<ModelStateValidationStartup<ModelStateDbContext>, ModelStateDbContext>>
    {
        private readonly IntegrationTestContext<ModelStateValidationStartup<ModelStateDbContext>, ModelStateDbContext> _testContext;

        public ModelStateValidationTests(IntegrationTestContext<ModelStateValidationStartup<ModelStateDbContext>, ModelStateDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task When_posting_resource_with_omitted_required_attribute_value_it_must_fail()
        {
            // Arrange
            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    attributes = new Dictionary<string, object>
                    {
                        ["industry"] = "Transport",
                        ["cityOfResidence"] = "Cambridge"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The CompanyName field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_posting_resource_with_null_for_required_attribute_value_it_must_fail()
        {
            // Arrange
            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = null,
                        ["industry"] = "Transport",
                        ["cityOfResidence"] = "Cambridge"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The CompanyName field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_posting_resource_with_invalid_attribute_value_it_must_fail()
        {
            // Arrange
            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "!@#$%^&*().-",
                        ["cityOfResidence"] = "Cambridge"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The field CompanyName must match the regular expression '^[\\w\\s]+$'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_posting_resource_with_valid_attribute_value_it_must_succeed()
        {
            // Arrange
            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "Massive Dynamic",
                        ["cityOfResidence"] = "Cambridge"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes["companyName"].Should().Be("Massive Dynamic");
            responseDocument.SingleData.Attributes["cityOfResidence"].Should().Be("Cambridge");
        }

        [Fact]
        public async Task When_posting_resource_with_multiple_violations_it_must_fail()
        {
            // Arrange
            var content = new
            {
                data = new
                {
                    type = "postalAddresses",
                    attributes = new Dictionary<string, object>
                    {
                        ["addressLine2"] = "X"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/postalAddresses";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(4);

            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The City field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/city");

            responseDocument.Errors[1].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[1].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[1].Detail.Should().Be("The Region field is required.");
            responseDocument.Errors[1].Source.Pointer.Should().Be("/data/attributes/region");

            responseDocument.Errors[2].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[2].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[2].Detail.Should().Be("The ZipCode field is required.");
            responseDocument.Errors[2].Source.Pointer.Should().Be("/data/attributes/zipCode");

            responseDocument.Errors[3].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[3].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[3].Detail.Should().Be("The StreetAddress field is required.");
            responseDocument.Errors[3].Source.Pointer.Should().Be("/data/attributes/streetAddress");
        }

        [Fact]
        public async Task When_posting_resource_with_annotated_relationships_it_must_succeed()
        {
            // Arrange
            var mailAddress = new PostalAddress
            {
                StreetAddress = "3555 S Las Vegas Blvd",
                City = "Las Vegas",
                Region = "Nevada",
                ZipCode = "89109"
            };

            var partner = new EnterprisePartner
            {
                Name = "Flamingo Casino",
                Classification = EnterprisePartnerClassification.Platinum,
                PrimaryMailAddress = mailAddress
            };

            var parent = new Enterprise
            {
                CompanyName = "Caesars Entertainment",
                CityOfResidence = "Las Vegas"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.EnterprisePartners.Add(partner);
                dbContext.Enterprises.Add(parent);

                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "Flamingo Hotel",
                        ["cityOfResidence"] = "Las Vegas"
                    },
                    relationships = new Dictionary<string, object>
                    {
                        ["mailAddress"] = new
                        {
                            data = new
                            {
                                type = "postalAddresses",
                                id = mailAddress.StringId
                            }
                        },
                        ["partners"] = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "partners",
                                    id = partner.StringId
                                }
                            }
                        },
                        ["parent"] = new
                        {
                            data = new
                            {
                                type = "enterprises",
                                id = parent.StringId
                            }
                        }
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes["companyName"].Should().Be("Flamingo Hotel");
        }

        [Fact]
        public async Task When_patching_resource_with_omitted_required_attribute_value_it_must_succeed()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Massive Dynamic",
                CityOfResidence = "Cambridge",
                Industry = "Manufacturing"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["industry"] = "Electronics"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }

        [Fact]
        public async Task When_patching_resource_with_null_for_required_attribute_value_it_must_fail()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Massive Dynamic",
                CityOfResidence = "Cambridge"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = null
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The CompanyName field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_patching_resource_with_invalid_attribute_value_it_must_fail()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Massive Dynamic",
                CityOfResidence = "Cambridge"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "!@#$%^&*().-"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The field CompanyName must match the regular expression '^[\\w\\s]+$'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/attributes/companyName");
        }

        [Fact]
        public async Task When_patching_resource_with_valid_attribute_value_it_must_succeed()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Massive Dynamic",
                CityOfResidence = "Cambridge",
                Industry = "Manufacturing"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "Umbrella Corporation"
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }

        [Fact]
        public async Task When_patching_resource_with_annotated_relationships_it_must_succeed()
        {
            // Arrange
            var mailAddress = new PostalAddress
            {
                StreetAddress = "Massachusetts Hall",
                City = "Cambridge",
                Region = "Massachusetts",
                ZipCode = "02138"
            };

            var enterprise = new Enterprise
            {
                CompanyName = "Bell Medics",
                CityOfResidence = "Cambridge",
                MailAddress = mailAddress,
                Partners = new List<EnterprisePartner>
                {
                    new EnterprisePartner
                    {
                        Name = "Harvard Laboratory",
                        Classification = EnterprisePartnerClassification.Silver,
                        PrimaryMailAddress = mailAddress
                    }
                },
                Parent = new Enterprise
                {
                    CompanyName = "Global Inc",
                    CityOfResidence = "New York"
                }
            };

            var otherMailAddress = new PostalAddress
            {
                StreetAddress = "2381  Burke Street",
                City = "Cambridge",
                Region = "Massachusetts",
                ZipCode = "02141"
            };

            var otherPartner = new EnterprisePartner
            {
                Name = "FBI",
                Classification = EnterprisePartnerClassification.Gold
            };

            var otherParent = new Enterprise
            {
                CompanyName = "World Inc",
                CityOfResidence = "New York"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.AddRange(enterprise, otherParent);
                dbContext.PostalAddresses.Add(otherMailAddress);
                dbContext.EnterprisePartners.Add(otherPartner);

                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "Massive Dynamic"
                    },
                    relationships = new Dictionary<string, object>
                    {
                        ["mailAddress"] = new
                        {
                            data = new
                            {
                                type = "postalAddresses",
                                id = otherMailAddress.StringId
                            }
                        },
                        ["partners"] = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "partners",
                                    id = otherPartner.StringId
                                }
                            }
                        },
                        ["parent"] = new
                        {
                            data = new
                            {
                                type = "enterprises",
                                id = otherParent.StringId
                            }
                        }
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }

        [Fact]
        public async Task When_patching_resource_with_multiple_self_references_it_must_succeed()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Bell Medics",
                CityOfResidence = "Cambridge",
                Parent = new Enterprise
                {
                    CompanyName = "Global Inc",
                    CityOfResidence = "New York"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = enterprise.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["companyName"] = "Massive Dynamic"
                    },
                    relationships = new Dictionary<string, object>
                    {
                        ["self"] = new
                        {
                            data = new
                            {
                                type = "enterprises",
                                id = enterprise.StringId
                            }
                        },
                        ["alsoSelf"] = new
                        {
                            data = new
                            {
                                type = "enterprises",
                                id = enterprise.StringId
                            }
                        }
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }

        [Fact]
        public async Task When_patching_annotated_ToOne_relationship_it_must_succeed()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Bell Medics",
                CityOfResidence = "Cambridge",
                Parent = new Enterprise
                {
                    CompanyName = "Global Inc",
                    CityOfResidence = "New York"
                }
            };

            var otherParent = new Enterprise
            {
                CompanyName = "World Inc",
                CityOfResidence = "New York"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.AddRange(enterprise, otherParent);
                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new
                {
                    type = "enterprises",
                    id = otherParent.StringId
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId + "/relationships/parent";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }

        [Fact]
        public async Task When_patching_annotated_ToMany_relationship_it_must_succeed()
        {
            // Arrange
            var enterprise = new Enterprise
            {
                CompanyName = "Bell Medics",
                CityOfResidence = "Cambridge",
                Partners = new List<EnterprisePartner>
                {
                    new EnterprisePartner
                    {
                        Name = "Harvard Laboratory",
                        Classification = EnterprisePartnerClassification.Silver,
                    }
                }
            };

            var otherPartner = new EnterprisePartner
            {
                Name = "FBI",
                Classification = EnterprisePartnerClassification.Gold
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Enterprises.Add(enterprise);
                dbContext.EnterprisePartners.Add(otherPartner);

                await dbContext.SaveChangesAsync();
            });

            var content = new
            {
                data = new[]
                {
                    new
                    {
                        type = "enterprisePartners",
                        id = otherPartner.StringId
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(content);
            string route = "/enterprises/" + enterprise.StringId + "/relationships/partners";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }
    }
}
