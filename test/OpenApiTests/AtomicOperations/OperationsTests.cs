using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.AtomicOperations;

public sealed class OperationsTests : IClassFixture<OpenApiTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;

    public OperationsTests(OpenApiTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CoursesController>();
        testContext.UseController<TeachersController>();
        testContext.UseController<StudentsController>();
        testContext.UseController<EnrollmentsController>();
        testContext.UseController<OperationsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeJsonApiVersion = true;

        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Fact]
    public async Task Operations_endpoint_is_exposed()
    {
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        document.Should().ContainPath("paths./operations.post").Should().BeJson("""
            {
              "tags": [
                "operations"
              ],
              "summary": "Performs multiple mutations in a linear and atomic manner.",
              "operationId": "postOperations",
              "requestBody": {
                "description": "An array of mutation operations. For syntax, see the [Atomic Operations documentation](https://jsonapi.org/ext/atomic/).",
                "content": {
                  "application/vnd.api+json; ext=atomic": {
                    "schema": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/operationsRequestDocument"
                        }
                      ]
                    }
                  }
                },
                "required": true
              },
              "responses": {
                "200": {
                  "description": "All operations were successfully applied, which resulted in additional changes.",
                  "content": {
                    "application/vnd.api+json; ext=atomic": {
                      "schema": {
                        "$ref": "#/components/schemas/operationsResponseDocument"
                      }
                    }
                  }
                },
                "204": {
                  "description": "All operations were successfully applied, which did not result in additional changes."
                },
                "400": {
                  "description": "The request body is missing or malformed.",
                  "content": {
                    "application/vnd.api+json; ext=atomic": {
                      "schema": {
                        "$ref": "#/components/schemas/errorResponseDocument"
                      }
                    }
                  }
                },
                "403": {
                  "description": "An operation is not accessible or a client-generated ID is used.",
                  "content": {
                    "application/vnd.api+json; ext=atomic": {
                      "schema": {
                        "$ref": "#/components/schemas/errorResponseDocument"
                      }
                    }
                  }
                },
                "404": {
                  "description": "A resource or a related resource does not exist.",
                  "content": {
                    "application/vnd.api+json; ext=atomic": {
                      "schema": {
                        "$ref": "#/components/schemas/errorResponseDocument"
                      }
                    }
                  }
                },
                "409": {
                  "description": "The request body contains conflicting information or another resource with the same ID already exists.",
                  "content": {
                    "application/vnd.api+json; ext=atomic": {
                      "schema": {
                        "$ref": "#/components/schemas/errorResponseDocument"
                      }
                    }
                  }
                },
                "422": {
                  "description": "Validation of the request body failed.",
                  "content": {
                    "application/vnd.api+json; ext=atomic": {
                      "schema": {
                        "$ref": "#/components/schemas/errorResponseDocument"
                      }
                    }
                  }
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Operations_request_component_schemas_are_exposed()
    {
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().ContainPath("operationsRequestDocument").Should().BeJson("""
                {
                  "required": [
                    "atomic:operations"
                  ],
                  "type": "object",
                  "properties": {
                    "atomic:operations": {
                      "minItems": 1,
                      "type": "array",
                      "items": {
                        "$ref": "#/components/schemas/atomicOperation"
                      }
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("atomicOperation").Should().BeJson("""
                {
                  "required": [
                    "openapi:discriminator"
                  ],
                  "type": "object",
                  "properties": {
                    "openapi:discriminator": {
                      "type": "string"
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false,
                  "discriminator": {
                    "propertyName": "openapi:discriminator",
                    "mapping": {
                      "addCourse": "#/components/schemas/createCourseOperation",
                      "addEnrollment": "#/components/schemas/createEnrollmentOperation",
                      "addStudent": "#/components/schemas/createStudentOperation",
                      "addTeacher": "#/components/schemas/createTeacherOperation",
                      "addToCourseEnrollments": "#/components/schemas/addToCourseEnrollmentsRelationshipOperation",
                      "addToCourseTaughtBy": "#/components/schemas/addToCourseTaughtByRelationshipOperation",
                      "addToStudentEnrollments": "#/components/schemas/addToStudentEnrollmentsRelationshipOperation",
                      "addToTeacherMentors": "#/components/schemas/addToTeacherMentorsRelationshipOperation",
                      "addToTeacherTeaches": "#/components/schemas/addToTeacherTeachesRelationshipOperation",
                      "removeCourse": "#/components/schemas/deleteCourseOperation",
                      "removeEnrollment": "#/components/schemas/deleteEnrollmentOperation",
                      "removeFromStudentEnrollments": "#/components/schemas/removeFromStudentEnrollmentsRelationshipOperation",
                      "removeFromTeacherMentors": "#/components/schemas/removeFromTeacherMentorsRelationshipOperation",
                      "removeFromTeacherTeaches": "#/components/schemas/removeFromTeacherTeachesRelationshipOperation",
                      "removeTeacher": "#/components/schemas/deleteTeacherOperation",
                      "updateCourse": "#/components/schemas/updateCourseOperation",
                      "updateCourseEnrollments": "#/components/schemas/updateCourseEnrollmentsRelationshipOperation",
                      "updateEnrollment": "#/components/schemas/updateEnrollmentOperation",
                      "updateEnrollmentCourse": "#/components/schemas/updateEnrollmentCourseRelationshipOperation",
                      "updateEnrollmentStudent": "#/components/schemas/updateEnrollmentStudentRelationshipOperation",
                      "updateStudent": "#/components/schemas/updateStudentOperation",
                      "updateStudentEnrollments": "#/components/schemas/updateStudentEnrollmentsRelationshipOperation",
                      "updateStudentMentor": "#/components/schemas/updateStudentMentorRelationshipOperation",
                      "updateTeacher": "#/components/schemas/updateTeacherOperation",
                      "updateTeacherMentors": "#/components/schemas/updateTeacherMentorsRelationshipOperation",
                      "updateTeacherTeaches": "#/components/schemas/updateTeacherTeachesRelationshipOperation"
                    }
                  },
                  "x-abstract": true
                }
                """);

            schemasElement.Should().ContainPath("addOperationCode").Should().BeJson("""
                {
                  "enum": [
                    "add"
                  ],
                  "type": "string"
                }
                """);

            schemasElement.Should().ContainPath("updateOperationCode").Should().BeJson("""
                {
                  "enum": [
                    "update"
                  ],
                  "type": "string"
                }
                """);

            schemasElement.Should().ContainPath("removeOperationCode").Should().BeJson("""
                {
                  "enum": [
                    "remove"
                  ],
                  "type": "string"
                }
                """);
        });
    }

    [Fact]
    public async Task Operations_response_component_schemas_are_exposed()
    {
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().ContainPath("operationsResponseDocument").Should().BeJson("""
                {
                  "required": [
                    "atomic:results",
                    "links"
                  ],
                  "type": "object",
                  "properties": {
                    "jsonapi": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/jsonapi"
                        }
                      ]
                    },
                    "links": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/resourceTopLevelLinks"
                        }
                      ]
                    },
                    "atomic:results": {
                      "minItems": 1,
                      "type": "array",
                      "items": {
                        "$ref": "#/components/schemas/atomicResult"
                      }
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("atomicResult").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "data": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/dataInResponse"
                        }
                      ]
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);
        });
    }

    [Fact]
    public async Task Course_operation_component_schemas_are_exposed()
    {
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            // resource operations
            schemasElement.Should().ContainPath("createCourseOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/addOperationCode"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/dataInCreateCourseRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("dataInCreateCourseRequest").Should().BeJson("""
                {
                  "required": [
                    "id",
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/courseResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "uuid"
                    },
                    "attributes": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/attributesInCreateCourseRequest"
                        }
                      ]
                    },
                    "relationships": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/relationshipsInCreateCourseRequest"
                        }
                      ]
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("attributesInCreateCourseRequest").Should().BeJson("""
                {
                  "required": [
                    "subject"
                  ],
                  "type": "object",
                  "properties": {
                    "subject": {
                      "type": "string"
                    },
                    "description": {
                      "type": "string",
                      "nullable": true
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("relationshipsInCreateCourseRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "taughtBy": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyTeacherInRequest"
                        }
                      ]
                    },
                    "enrollments": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyEnrollmentInRequest"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("updateCourseOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/updateOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/courseIdentifierInRequest"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/dataInUpdateCourseRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("dataInUpdateCourseRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/courseResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "uuid"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "attributes": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/attributesInUpdateCourseRequest"
                        }
                      ]
                    },
                    "relationships": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/relationshipsInUpdateCourseRequest"
                        }
                      ]
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("attributesInUpdateCourseRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "subject": {
                      "type": "string"
                    },
                    "description": {
                      "type": "string",
                      "nullable": true
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("relationshipsInUpdateCourseRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "taughtBy": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyTeacherInRequest"
                        }
                      ]
                    },
                    "enrollments": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyEnrollmentInRequest"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("deleteCourseOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/removeOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/courseIdentifierInRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            // relationship operations
            schemasElement.Should().ContainPath("updateCourseEnrollmentsRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/updateOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/courseEnrollmentsRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "type": "array",
                          "items": {
                            "$ref": "#/components/schemas/enrollmentIdentifierInRequest"
                          }
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("addToCourseEnrollmentsRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/addOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/courseEnrollmentsRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "type": "array",
                          "items": {
                            "$ref": "#/components/schemas/enrollmentIdentifierInRequest"
                          }
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().NotContainPath("removeFromCourseEnrollmentsRelationshipOperation");

            schemasElement.Should().NotContainPath("updateCourseTaughtByRelationshipOperation");

            // shared types
            schemasElement.Should().ContainPath("courseIdentifierInRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/courseResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "uuid"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("courseEnrollmentsRelationshipIdentifier").Should().BeJson("""
                {
                  "required": [
                    "relationship",
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/courseResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "uuid"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "relationship": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/courseEnrollmentsRelationshipName"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("courseEnrollmentsRelationshipName").Should().BeJson("""
                {
                  "enum": [
                    "enrollments"
                  ],
                  "type": "string"
                }
                """);
        });
    }

    [Fact]
    public async Task Student_operation_component_schemas_are_exposed()
    {
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            // resource operations
            schemasElement.Should().ContainPath("createStudentOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/addOperationCode"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/dataInCreateStudentRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("dataInCreateStudentRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/studentResourceType"
                        }
                      ]
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "attributes": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/attributesInCreateStudentRequest"
                        }
                      ]
                    },
                    "relationships": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/relationshipsInCreateStudentRequest"
                        }
                      ]
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("attributesInCreateStudentRequest").Should().BeJson("""
                {
                  "required": [
                    "name"
                  ],
                  "type": "object",
                  "properties": {
                    "name": {
                      "type": "string"
                    },
                    "emailAddress": {
                      "type": "string",
                      "nullable": true
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("relationshipsInCreateStudentRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "mentor": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/nullableToOneTeacherInRequest"
                        }
                      ]
                    },
                    "enrollments": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyEnrollmentInRequest"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("updateStudentOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/updateOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/studentIdentifierInRequest"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/dataInUpdateStudentRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("dataInUpdateStudentRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/studentResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "attributes": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/attributesInUpdateStudentRequest"
                        }
                      ]
                    },
                    "relationships": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/relationshipsInUpdateStudentRequest"
                        }
                      ]
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("attributesInUpdateStudentRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "name": {
                      "type": "string"
                    },
                    "emailAddress": {
                      "type": "string",
                      "nullable": true
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("relationshipsInUpdateStudentRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "mentor": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/nullableToOneTeacherInRequest"
                        }
                      ]
                    },
                    "enrollments": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyEnrollmentInRequest"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().NotContainPath("deleteStudentOperation");

            // relationship operations
            schemasElement.Should().ContainPath("updateStudentMentorRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/updateOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/studentMentorRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/teacherIdentifierInRequest"
                            }
                          ],
                          "nullable": true
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("updateStudentEnrollmentsRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/updateOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/studentEnrollmentsRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "type": "array",
                          "items": {
                            "$ref": "#/components/schemas/enrollmentIdentifierInRequest"
                          }
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("addToStudentEnrollmentsRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/addOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/studentEnrollmentsRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "type": "array",
                          "items": {
                            "$ref": "#/components/schemas/enrollmentIdentifierInRequest"
                          }
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("removeFromStudentEnrollmentsRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/removeOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/studentEnrollmentsRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "type": "array",
                          "items": {
                            "$ref": "#/components/schemas/enrollmentIdentifierInRequest"
                          }
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            // shared types
            schemasElement.Should().ContainPath("studentIdentifierInRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/studentResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("studentMentorRelationshipIdentifier").Should().BeJson("""
                {
                  "required": [
                    "relationship",
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/studentResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "relationship": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/studentMentorRelationshipName"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("studentMentorRelationshipName").Should().BeJson("""
                {
                  "enum": [
                    "mentor"
                  ],
                  "type": "string"
                }
                """);

            schemasElement.Should().ContainPath("studentEnrollmentsRelationshipIdentifier").Should().BeJson("""
                {
                  "required": [
                    "relationship",
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/studentResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "relationship": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/studentEnrollmentsRelationshipName"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("studentEnrollmentsRelationshipName").Should().BeJson("""
                {
                  "enum": [
                    "enrollments"
                  ],
                  "type": "string"
                }
                """);
        });
    }

    [Fact]
    public async Task Teacher_operation_component_schemas_are_exposed()
    {
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            // resource operations
            schemasElement.Should().ContainPath("createTeacherOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/addOperationCode"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/dataInCreateTeacherRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("dataInCreateTeacherRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/teacherResourceType"
                        }
                      ]
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "attributes": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/attributesInCreateTeacherRequest"
                        }
                      ]
                    },
                    "relationships": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/relationshipsInCreateTeacherRequest"
                        }
                      ]
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("attributesInCreateTeacherRequest").Should().BeJson("""
                {
                  "required": [
                    "name"
                  ],
                  "type": "object",
                  "properties": {
                    "name": {
                      "type": "string"
                    },
                    "emailAddress": {
                      "type": "string",
                      "nullable": true
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("relationshipsInCreateTeacherRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "teaches": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyCourseInRequest"
                        }
                      ]
                    },
                    "mentors": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyStudentInRequest"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("updateTeacherOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/updateOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/teacherIdentifierInRequest"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/dataInUpdateTeacherRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("dataInUpdateTeacherRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/teacherResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "attributes": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/attributesInUpdateTeacherRequest"
                        }
                      ]
                    },
                    "relationships": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/relationshipsInUpdateTeacherRequest"
                        }
                      ]
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("attributesInUpdateTeacherRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "name": {
                      "type": "string"
                    },
                    "emailAddress": {
                      "type": "string",
                      "nullable": true
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("relationshipsInUpdateTeacherRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "teaches": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyCourseInRequest"
                        }
                      ]
                    },
                    "mentors": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toManyStudentInRequest"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("deleteTeacherOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/removeOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/teacherIdentifierInRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            // relationship operations
            schemasElement.Should().ContainPath("updateTeacherMentorsRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/updateOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/teacherMentorsRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "type": "array",
                          "items": {
                            "$ref": "#/components/schemas/studentIdentifierInRequest"
                          }
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("addToTeacherMentorsRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/addOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/teacherMentorsRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "type": "array",
                          "items": {
                            "$ref": "#/components/schemas/studentIdentifierInRequest"
                          }
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("removeFromTeacherMentorsRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/removeOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/teacherMentorsRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "type": "array",
                          "items": {
                            "$ref": "#/components/schemas/studentIdentifierInRequest"
                          }
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            // shared types
            schemasElement.Should().ContainPath("teacherIdentifierInRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/teacherResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("teacherMentorsRelationshipIdentifier").Should().BeJson("""
                {
                  "required": [
                    "relationship",
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/teacherResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "relationship": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/teacherMentorsRelationshipName"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("teacherMentorsRelationshipName").Should().BeJson("""
                {
                  "enum": [
                    "mentors"
                  ],
                  "type": "string"
                }
                """);
        });
    }

    [Fact]
    public async Task Enrollment_operation_component_schemas_are_exposed()
    {
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            // resource operations
            schemasElement.Should().ContainPath("createEnrollmentOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/addOperationCode"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/dataInCreateEnrollmentRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("dataInCreateEnrollmentRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/enrollmentResourceType"
                        }
                      ]
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "attributes": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/attributesInCreateEnrollmentRequest"
                        }
                      ]
                    },
                    "relationships": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/relationshipsInCreateEnrollmentRequest"
                        }
                      ]
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("attributesInCreateEnrollmentRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "enrolledAt": {
                      "type": "string",
                      "format": "date"
                    },
                    "graduatedAt": {
                      "type": "string",
                      "format": "date",
                      "nullable": true
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("relationshipsInCreateEnrollmentRequest").Should().BeJson("""
                {
                  "required": [
                    "course",
                    "student"
                  ],
                  "type": "object",
                  "properties": {
                    "student": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toOneStudentInRequest"
                        }
                      ]
                    },
                    "course": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toOneCourseInRequest"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("updateEnrollmentOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/updateOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/enrollmentIdentifierInRequest"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/dataInUpdateEnrollmentRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("dataInUpdateEnrollmentRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/enrollmentResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "attributes": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/attributesInUpdateEnrollmentRequest"
                        }
                      ]
                    },
                    "relationships": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/relationshipsInUpdateEnrollmentRequest"
                        }
                      ]
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("attributesInUpdateEnrollmentRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "enrolledAt": {
                      "type": "string",
                      "format": "date"
                    },
                    "graduatedAt": {
                      "type": "string",
                      "format": "date",
                      "nullable": true
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("relationshipsInUpdateEnrollmentRequest").Should().BeJson("""
                {
                  "type": "object",
                  "properties": {
                    "student": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toOneStudentInRequest"
                        }
                      ]
                    },
                    "course": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/toOneCourseInRequest"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("deleteEnrollmentOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/removeOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/enrollmentIdentifierInRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            // relationship operations
            schemasElement.Should().ContainPath("updateEnrollmentCourseRelationshipOperation").Should().BeJson("""
                {
                  "allOf": [
                    {
                      "$ref": "#/components/schemas/atomicOperation"
                    },
                    {
                      "required": [
                        "data",
                        "op",
                        "ref"
                      ],
                      "type": "object",
                      "properties": {
                        "op": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/updateOperationCode"
                            }
                          ]
                        },
                        "ref": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/enrollmentCourseRelationshipIdentifier"
                            }
                          ]
                        },
                        "data": {
                          "allOf": [
                            {
                              "$ref": "#/components/schemas/courseIdentifierInRequest"
                            }
                          ]
                        }
                      },
                      "additionalProperties": false
                    }
                  ],
                  "additionalProperties": false
                }
                """);

            // shared types
            schemasElement.Should().ContainPath("enrollmentIdentifierInRequest").Should().BeJson("""
                {
                  "required": [
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/enrollmentResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "meta": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/meta"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("enrollmentCourseRelationshipIdentifier").Should().BeJson("""
                {
                  "required": [
                    "relationship",
                    "type"
                  ],
                  "type": "object",
                  "properties": {
                    "type": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/enrollmentResourceType"
                        }
                      ]
                    },
                    "id": {
                      "minLength": 1,
                      "type": "string",
                      "format": "int64"
                    },
                    "lid": {
                      "minLength": 1,
                      "type": "string"
                    },
                    "relationship": {
                      "allOf": [
                        {
                          "$ref": "#/components/schemas/enrollmentCourseRelationshipName"
                        }
                      ]
                    }
                  },
                  "additionalProperties": false
                }
                """);

            schemasElement.Should().ContainPath("enrollmentCourseRelationshipName").Should().BeJson("""
                {
                  "enum": [
                    "course"
                  ],
                  "type": "string"
                }
                """);
        });
    }
}
