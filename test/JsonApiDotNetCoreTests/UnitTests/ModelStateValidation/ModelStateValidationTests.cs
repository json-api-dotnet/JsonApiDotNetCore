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

namespace JsonApiDotNetCoreTests.UnitTests.ModelStateValidation;

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
    [InlineData("ComplexObject.Elements[0]", "/data/attributes/publicNameOfComplexObject/jsonElements/0")]
    [InlineData("ComplexObject.Elements[0].First", "/data/attributes/publicNameOfComplexObject/jsonElements/0/jsonFirst")]
    [InlineData("[ComplexObject][Elements][0][First]", "/data/attributes/publicNameOfComplexObject/jsonElements/0/jsonFirst")]
    [InlineData("ComplexList", "/data/attributes/publicNameOfComplexList")]
    [InlineData("ComplexList[0].First", "/data/attributes/publicNameOfComplexList/0/jsonFirst")]
    [InlineData("ComplexObject.NoSuchMember", null)]
    [InlineData("GenericObject.Value", "/data/attributes/publicNameOfGenericObject/jsonValue")]
    [InlineData("GenericList[1].Value", "/data/attributes/publicNameOfGenericList/1/jsonValue")]
    [InlineData("PlainObject", "/data/attributes/publicNameOfPlainObject")]
    [InlineData("PlainObject.Value", "/data/attributes/publicNameOfPlainObject/Value")]
    [InlineData("PlainObject.NoSuchMember", null)]
    [InlineData("PrimaryChild", "/data/relationships/publicNameOfPrimaryChild/data")]
    [InlineData("PrimaryChild.NotMappedInChild", null)]
    [InlineData("PrimaryChild.Id", "/data/relationships/publicNameOfPrimaryChild/data/id")]
    [InlineData("Children[0]", "/data/relationships/publicNameOfChildren/data/0")]
    [InlineData("Children[0].NotMappedInChild", null)]
    [InlineData("Children[0].Id", "/data/relationships/publicNameOfChildren/data/0/id")]
    public void Renders_JSON_path_for_ModelState_key_in_resource_request(string modelStateKey, string? expectedJsonPath)
    {
        // Arrange
        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Parent, long>().Add<Child, long>().Build();

        var modelState = new ModelStateDictionary();
        modelState.AddModelError(modelStateKey, "(ignored error message)");

        // Act
        var exception = new InvalidModelStateException(modelState, typeof(Parent), false, resourceGraph);

        // Assert
        exception.Errors.Should().HaveCount(1);

        if (expectedJsonPath == null)
        {
            exception.Errors[0].Source.Should().BeNull();
        }
        else
        {
            exception.Errors[0].Source.RefShould().NotBeNull().And.Subject.Pointer.Should().Be(expectedJsonPath);
        }
    }

    [Theory]
    [InlineData("[0]", "/atomic:operations/0")]
    [InlineData("[0].Resource", null)]
    [InlineData("[0].Resource.NotMappedInParent", null)]
    [InlineData("[0].Resource.Id", "/atomic:operations/0/data/id")]
    [InlineData("[0].Resource.One", "/atomic:operations/0/data/attributes/publicNameOfOne")]
    [InlineData("[0].Resource.ComplexObject", "/atomic:operations/0/data/attributes/publicNameOfComplexObject")]
    [InlineData("[0].Resource.ComplexObject.First", "/atomic:operations/0/data/attributes/publicNameOfComplexObject/jsonFirst")]
    [InlineData("[0].Resource.ComplexObject.ParentObject.First", "/atomic:operations/0/data/attributes/publicNameOfComplexObject/jsonParentObject/jsonFirst")]
    [InlineData("[0].Resource.ComplexObject.Elements[0]", "/atomic:operations/0/data/attributes/publicNameOfComplexObject/jsonElements/0")]
    [InlineData("[0].Resource.ComplexObject.Elements[0].First", "/atomic:operations/0/data/attributes/publicNameOfComplexObject/jsonElements/0/jsonFirst")]
    [InlineData("[0][Resource][ComplexObject][Elements][0][First]", "/atomic:operations/0/data/attributes/publicNameOfComplexObject/jsonElements/0/jsonFirst")]
    [InlineData("[0].Resource.ComplexList", "/atomic:operations/0/data/attributes/publicNameOfComplexList")]
    [InlineData("[0].Resource.ComplexList[0].First", "/atomic:operations/0/data/attributes/publicNameOfComplexList/0/jsonFirst")]
    [InlineData("[0].Resource.ComplexObject.NoSuchMember", null)]
    [InlineData("[1].Resource.GenericObject.Value", "/atomic:operations/1/data/attributes/publicNameOfGenericObject/jsonValue")]
    [InlineData("[0].Resource.GenericList[1].Value", "/atomic:operations/0/data/attributes/publicNameOfGenericList/1/jsonValue")]
    [InlineData("[0].Resource.PlainObject.Value", "/atomic:operations/0/data/attributes/publicNameOfPlainObject/Value")]
    [InlineData("[0].Resource.PrimaryChild", "/atomic:operations/0/data/relationships/publicNameOfPrimaryChild/data")]
    [InlineData("[0].Resource.PrimaryChild.NotMappedInChild", null)]
    [InlineData("[0].Resource.PrimaryChild.Id", "/atomic:operations/0/data/relationships/publicNameOfPrimaryChild/data/id")]
    [InlineData("[0].Resource.Children[0]", "/atomic:operations/0/data/relationships/publicNameOfChildren/data/0")]
    [InlineData("[0].Resource.Children[0].NotMappedInChild", null)]
    [InlineData("[0].Resource.Children[0].Id", "/atomic:operations/0/data/relationships/publicNameOfChildren/data/0/id")]
    public void Renders_JSON_path_for_ModelState_key_in_operations_request(string modelStateKey, string? expectedJsonPath)
    {
        // Arrange
        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Parent, long>().Add<Child, long>().Build();

        var modelState = new ModelStateDictionary();
        modelState.AddModelError(modelStateKey, "(ignored error message)");

        Func<Type, int, Type?> getOperationTypeCallback = (collectionType, _) => collectionType == typeof(IList<OperationContainer>) ? typeof(Parent) : null;

        // Act
        var exception = new InvalidModelStateException(modelState, typeof(IList<OperationContainer>), false, resourceGraph, getOperationTypeCallback);

        // Assert
        exception.Errors.Should().HaveCount(1);

        if (expectedJsonPath == null)
        {
            exception.Errors[0].Source.Should().BeNull();
        }
        else
        {
            exception.Errors[0].Source.RefShould().NotBeNull().And.Subject.Pointer.Should().Be(expectedJsonPath);
        }
    }

    [Fact(Skip = "KNOWN-ISSUE D5: FindCompoundType(Nullable<T>) returns null, so no source pointer is produced for members of a nullable struct compound")]
    public void Renders_JSON_path_for_ModelState_key_in_nullable_struct_compound()
    {
        // Arrange
        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<StructParent, long>().Build();

        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Location.Latitude", "(ignored error message)");

        // Act
        var exception = new InvalidModelStateException(modelState, typeof(StructParent), false, resourceGraph);

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors[0].Source.RefShould().NotBeNull().And.Subject.Pointer.Should().Be("/data/attributes/publicNameOfLocation/jsonLatitude");
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Parent : Identifiable<long>
    {
        public string? NotMappedInParent { get; set; }

        [Attr(PublicName = "publicNameOfOne")]
        public string? One { get; set; }

        [Attr(PublicName = "publicNameOfComplexObject", IsCompound = true)]
        public ComplexRootObject? ComplexObject { get; set; }

        [Attr(PublicName = "publicNameOfComplexList", IsCompound = true)]
        public IList<ComplexElementObject> ComplexList { get; set; } = null!;

        [Attr(PublicName = "publicNameOfGenericObject", IsCompound = true)]
        public GenericCompound<string>? GenericObject { get; set; }

        [Attr(PublicName = "publicNameOfGenericList", IsCompound = true)]
        public IList<GenericCompound<DateOnly>> GenericList { get; set; } = null!;

        [Attr(PublicName = "publicNameOfPlainObject")]
        public PlainComplexObject? PlainObject { get; set; }

        [HasOne(PublicName = "publicNameOfPrimaryChild")]
        public Child? PrimaryChild { get; set; }

        [HasMany(PublicName = "publicNameOfChildren")]
        public ISet<Child> Children { get; set; } = null!;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Child : Identifiable<long>
    {
        public string? NotMappedInChild { get; set; }

        [Attr(PublicName = "publicNameOfTwo")]
        public string? Two { get; set; }

        [HasOne(PublicName = "publicNameOfParent")]
        public Parent? Parent { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ComplexRootObject
    {
        [Attr(PublicName = "jsonFirst")]
        public string? First { get; set; }

        [Attr(PublicName = "jsonParentObject", IsCompound = true)]
        public ComplexParentObject? ParentObject { get; set; }

        [Attr(PublicName = "jsonElements", IsCompound = true)]
        public IList<ComplexElementObject> Elements { get; set; } = null!;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ComplexParentObject
    {
        [Attr(PublicName = "jsonFirst")]
        public string? First { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ComplexElementObject
    {
        [Attr(PublicName = "jsonFirst")]
        public string? First { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class GenericCompound<T>
    {
        [Attr(PublicName = "jsonValue")]
        public T? Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class PlainComplexObject
    {
        public string? Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class StructParent : Identifiable<long>
    {
        [Attr(PublicName = "publicNameOfLocation", IsCompound = true)]
        public GeoPoint? Location { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private struct GeoPoint
    {
        [Attr(PublicName = "jsonLatitude")]
        public double Latitude { get; set; }
    }
}
