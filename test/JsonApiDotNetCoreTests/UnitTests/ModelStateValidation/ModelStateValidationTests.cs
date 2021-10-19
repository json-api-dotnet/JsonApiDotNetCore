using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ModelStateValidation
{
    public sealed class ModelStateValidationTests
    {
        [Theory]
        [InlineData("", null)]
        [InlineData("NotMappedInParent", null)]
        [InlineData("Id", "/data/id")]
        [InlineData("One", "/data/attributes/publicNameOfOne")]
        [InlineData("ComplexObject", "/data/attributes/publicNameOfComplexObject")]
        [InlineData("ComplexObject.First", "/data/attributes/publicNameOfComplexObject/jsonFirst")]
        [InlineData("ComplexObject.ParentObject.First", "/data/attributes/publicNameOfComplexObject/jsonParentObject/jsonFirst")]
        [InlineData("ComplexObject.Elements[0]", "/data/attributes/publicNameOfComplexObject/jsonElements[0]")]
        [InlineData("ComplexObject.Elements[0].First", "/data/attributes/publicNameOfComplexObject/jsonElements[0]/jsonFirst")]
        [InlineData("[ComplexObject][Elements][0][First]", "/data/attributes/publicNameOfComplexObject/jsonElements[0]/jsonFirst")]
        [InlineData("ComplexList", "/data/attributes/publicNameOfComplexList")]
        [InlineData("ComplexList[0].First", "/data/attributes/publicNameOfComplexList[0]/jsonFirst")]
        [InlineData("PrimaryChild", "/data/relationships/publicNameOfPrimaryChild/data")]
        [InlineData("PrimaryChild.NotMappedInChild", null)]
        [InlineData("PrimaryChild.Id", "/data/relationships/publicNameOfPrimaryChild/data/id")]
        [InlineData("Children[0]", "/data/relationships/publicNameOfChildren/data[0]")]
        [InlineData("Children[0].NotMappedInChild", null)]
        [InlineData("Children[0].Id", "/data/relationships/publicNameOfChildren/data[0]/id")]
        public void Renders_JSON_path_for_ModelState_key_in_resource_request(string modelStateKey, string? expectedJsonPath)
        {
            // Arrange
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Parent>().Add<Child>().Build();

            var modelState = new ModelStateDictionary();
            modelState.AddModelError(modelStateKey, "(ignored error message)");

            // Act
            var exception = new InvalidModelStateException(modelState, typeof(Parent), false, resourceGraph);

            // Assert
            exception.Errors.ShouldHaveCount(1);

            if (expectedJsonPath == null)
            {
                exception.Errors[0].Source.Should().BeNull();
            }
            else
            {
                exception.Errors[0].Source.ShouldNotBeNull();
                exception.Errors[0].Source!.Pointer.Should().Be(expectedJsonPath);
            }
        }

        [Theory]
        [InlineData("[0]", "/atomic:operations[0]")]
        [InlineData("[0].Resource", null)]
        [InlineData("[0].Resource.NotMappedInParent", null)]
        [InlineData("[0].Resource.Id", "/atomic:operations[0]/data/id")]
        [InlineData("[0].Resource.One", "/atomic:operations[0]/data/attributes/publicNameOfOne")]
        [InlineData("[0].Resource.ComplexObject", "/atomic:operations[0]/data/attributes/publicNameOfComplexObject")]
        [InlineData("[0].Resource.ComplexObject.First", "/atomic:operations[0]/data/attributes/publicNameOfComplexObject/jsonFirst")]
        [InlineData("[0].Resource.ComplexObject.ParentObject.First",
            "/atomic:operations[0]/data/attributes/publicNameOfComplexObject/jsonParentObject/jsonFirst")]
        [InlineData("[0].Resource.ComplexObject.Elements[0]", "/atomic:operations[0]/data/attributes/publicNameOfComplexObject/jsonElements[0]")]
        [InlineData("[0].Resource.ComplexObject.Elements[0].First",
            "/atomic:operations[0]/data/attributes/publicNameOfComplexObject/jsonElements[0]/jsonFirst")]
        [InlineData("[0][Resource][ComplexObject][Elements][0][First]",
            "/atomic:operations[0]/data/attributes/publicNameOfComplexObject/jsonElements[0]/jsonFirst")]
        [InlineData("[0].Resource.ComplexList", "/atomic:operations[0]/data/attributes/publicNameOfComplexList")]
        [InlineData("[0].Resource.ComplexList[0].First", "/atomic:operations[0]/data/attributes/publicNameOfComplexList[0]/jsonFirst")]
        [InlineData("[0].Resource.PrimaryChild", "/atomic:operations[0]/data/relationships/publicNameOfPrimaryChild/data")]
        [InlineData("[0].Resource.PrimaryChild.NotMappedInChild", null)]
        [InlineData("[0].Resource.PrimaryChild.Id", "/atomic:operations[0]/data/relationships/publicNameOfPrimaryChild/data/id")]
        [InlineData("[0].Resource.Children[0]", "/atomic:operations[0]/data/relationships/publicNameOfChildren/data[0]")]
        [InlineData("[0].Resource.Children[0].NotMappedInChild", null)]
        [InlineData("[0].Resource.Children[0].Id", "/atomic:operations[0]/data/relationships/publicNameOfChildren/data[0]/id")]
        public void Renders_JSON_path_for_ModelState_key_in_operations_request(string modelStateKey, string? expectedJsonPath)
        {
            // Arrange
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Parent>().Add<Child>().Build();

            var modelState = new ModelStateDictionary();
            modelState.AddModelError(modelStateKey, "(ignored error message)");

            Func<Type, int, Type?> getOperationTypeCallback = (collectionType, _) =>
                collectionType == typeof(IList<OperationContainer>) ? typeof(Parent) : null;

            // Act
            var exception = new InvalidModelStateException(modelState, typeof(IList<OperationContainer>), false, resourceGraph, getOperationTypeCallback);

            // Assert
            exception.Errors.ShouldHaveCount(1);

            if (expectedJsonPath == null)
            {
                exception.Errors[0].Source.Should().BeNull();
            }
            else
            {
                exception.Errors[0].Source.ShouldNotBeNull();
                exception.Errors[0].Source!.Pointer.Should().Be(expectedJsonPath);
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class Parent : Identifiable<int>
        {
            public string? NotMappedInParent { get; set; }

            [Attr(PublicName = "publicNameOfOne")]
            public string? One { get; set; }

            [Attr(PublicName = "publicNameOfComplexObject")]
            public ComplexObject? ComplexObject { get; set; }

            [Attr(PublicName = "publicNameOfComplexList")]
            public IList<ComplexObject> ComplexList { get; set; } = null!;

            [HasOne(PublicName = "publicNameOfPrimaryChild")]
            public Child? PrimaryChild { get; set; }

            [HasMany(PublicName = "publicNameOfChildren")]
            public ISet<Child> Children { get; set; } = null!;
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class Child : Identifiable<int>
        {
            public string? NotMappedInChild { get; set; }

            [Attr(PublicName = "publicNameOfTwo")]
            public string? Two { get; set; }

            [HasOne(PublicName = "publicNameOfParent")]
            public Parent? Parent { get; set; }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class ComplexObject
        {
            [JsonPropertyName("jsonFirst")]
            public string? First { get; set; }

            [JsonPropertyName("jsonParentObject")]
            public ComplexObject? ParentObject { get; set; }

            [JsonPropertyName("jsonElements")]
            public IList<ComplexObject> Elements { get; set; } = null!;
        }
    }
}
