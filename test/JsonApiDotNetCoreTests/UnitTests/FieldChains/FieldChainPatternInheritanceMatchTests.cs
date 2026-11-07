using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace JsonApiDotNetCoreTests.UnitTests.FieldChains;

public sealed class FieldChainPatternInheritanceMatchTests : IDisposable
{
    private const string TBase = "bases";
    private const string TDerivedQ = "derivedQs";
    private const string TDerivedV = "derivedVs";

    private const string X = "unknown";
    private const string BaseA = "value";
    private const string BaseO = "singleItem";
    private const string BaseM = "setOfItems";

    private const string DerivedA = "derivedValue";
    private const string DerivedQA = "derivedQValue";
    private const string DerivedVA = "derivedVValue";
    private const string DerivedO = "derivedSingleItem";
    private const string DerivedQO = "derivedQSingleItem";
    private const string DerivedVO = "derivedVSingleItem";
    private const string DerivedM = "derivedSetOfItems";
    private const string DerivedQM = "derivedQSetOfItems";
    private const string DerivedVM = "derivedVSetOfItems";
    private const string DerivedToOneOrMany = "derivedToOneOrToMany";
    private const string DerivedQC = "derivedQInfo";
    private const string DerivedQQ = "derivedQPhones";
    private const string DerivedVL = "derivedVTags";
    private const string SharedC = "sharedInfo";
    private const string InfoDetail = "detail";

    private readonly LoggerFactory _loggerFactory;
    private readonly IResourceGraph _resourceGraph;

    public FieldChainPatternInheritanceMatchTests(ITestOutputHelper testOutputHelper)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        // Justification: LoggerFactory.AddProvider takes ownership (passing the provider as a constructor parameter does not).
        var loggerProvider = new XUnitLoggerProvider(testOutputHelper, null, LogOutputFields.Message);
#pragma warning restore CA2000 // Dispose objects before losing scope

        _loggerFactory = new LoggerFactory();
        _loggerFactory.AddProvider(loggerProvider);

        var options = new JsonApiOptions();
        _resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Base, long>().Add<DerivedQ, long>().Add<DerivedV, long>().Build();
    }

    [Theory]
    [InlineData("A", TBase, BaseA, typeof(Base))]
    [InlineData("F", TBase, BaseA, typeof(Base))]
    [InlineData("F", TBase, DerivedQA, typeof(DerivedQ))]
    [InlineData("F", TBase, DerivedVA, typeof(DerivedV))]
    [InlineData("O", TBase, BaseO, typeof(Base))]
    [InlineData("R", TBase, BaseO, typeof(Base))]
    [InlineData("F", TBase, BaseO, typeof(Base))]
    [InlineData("F", TBase, DerivedQO, typeof(DerivedQ))]
    [InlineData("F", TBase, DerivedVO, typeof(DerivedV))]
    [InlineData("M", TBase, BaseM, typeof(Base))]
    [InlineData("R", TBase, BaseM, typeof(Base))]
    [InlineData("F", TBase, BaseM, typeof(Base))]
    [InlineData("F", TBase, DerivedQM, typeof(DerivedQ))]
    [InlineData("F", TBase, DerivedVM, typeof(DerivedV))]
    [InlineData("A", TDerivedQ, BaseA, typeof(DerivedQ))]
    [InlineData("F", TDerivedQ, BaseA, typeof(DerivedQ))]
    [InlineData("A", TDerivedQ, DerivedA, typeof(DerivedQ))]
    [InlineData("F", TDerivedQ, DerivedA, typeof(DerivedQ))]
    [InlineData("O", TDerivedQ, BaseO, typeof(DerivedQ))]
    [InlineData("R", TDerivedQ, BaseO, typeof(DerivedQ))]
    [InlineData("F", TDerivedQ, BaseO, typeof(DerivedQ))]
    [InlineData("O", TDerivedQ, DerivedO, typeof(DerivedQ))]
    [InlineData("R", TDerivedQ, DerivedO, typeof(DerivedQ))]
    [InlineData("F", TDerivedQ, DerivedO, typeof(DerivedQ))]
    [InlineData("O", TDerivedQ, DerivedToOneOrMany, typeof(DerivedQ))]
    [InlineData("M", TDerivedQ, BaseM, typeof(DerivedQ))]
    [InlineData("R", TDerivedQ, BaseM, typeof(DerivedQ))]
    [InlineData("F", TDerivedQ, BaseM, typeof(DerivedQ))]
    [InlineData("M", TDerivedQ, DerivedM, typeof(DerivedQ))]
    [InlineData("R", TDerivedQ, DerivedM, typeof(DerivedQ))]
    [InlineData("F", TDerivedQ, DerivedM, typeof(DerivedQ))]
    [InlineData("A", TDerivedV, BaseA, typeof(DerivedV))]
    [InlineData("F", TDerivedV, BaseA, typeof(DerivedV))]
    [InlineData("A", TDerivedV, DerivedA, typeof(DerivedV))]
    [InlineData("F", TDerivedV, DerivedA, typeof(DerivedV))]
    [InlineData("O", TDerivedV, BaseO, typeof(DerivedV))]
    [InlineData("R", TDerivedV, BaseO, typeof(DerivedV))]
    [InlineData("F", TDerivedV, BaseO, typeof(DerivedV))]
    [InlineData("O", TDerivedV, DerivedO, typeof(DerivedV))]
    [InlineData("R", TDerivedV, DerivedO, typeof(DerivedV))]
    [InlineData("F", TDerivedV, DerivedO, typeof(DerivedV))]
    [InlineData("M", TDerivedV, BaseM, typeof(DerivedV))]
    [InlineData("R", TDerivedV, BaseM, typeof(DerivedV))]
    [InlineData("F", TDerivedV, BaseM, typeof(DerivedV))]
    [InlineData("M", TDerivedV, DerivedM, typeof(DerivedV))]
    [InlineData("R", TDerivedV, DerivedM, typeof(DerivedV))]
    [InlineData("F", TDerivedV, DerivedM, typeof(DerivedV))]
    [InlineData("M", TDerivedV, DerivedToOneOrMany, typeof(DerivedV))]
    [InlineData("C", TBase, DerivedQC, typeof(DerivedQ))]
    [InlineData("A", TBase, DerivedQC, typeof(DerivedQ))]
    [InlineData("F", TBase, DerivedQC, typeof(DerivedQ))]
    [InlineData("Q", TBase, DerivedQQ, typeof(DerivedQ))]
    [InlineData("L", TBase, DerivedVL, typeof(DerivedV))]
    [InlineData("C", TDerivedQ, DerivedQC, typeof(DerivedQ))]
    [InlineData("C", TDerivedQ, SharedC, typeof(DerivedQ))]
    [InlineData("C", TDerivedV, SharedC, typeof(DerivedV))]
    public void MatchSucceeds(string patternText, string resourceTypeName, string fieldChainText, Type expectedType)
    {
        // Arrange
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);
        ResourceType resourceType = _resourceGraph.GetResourceType(resourceTypeName);

        // Act
        PatternMatchResult result = pattern.Match(fieldChainText, resourceType, FieldChainPatternMatchOptions.AllowDerivedTypes, _loggerFactory);

        // Assert
        result.FailureMessage.Should().BeEmpty();
        result.FieldChain.Should().HaveCount(1);
        result.IsSuccess.Should().BeTrue();

        result.FieldChain[0].PublicName.Should().Be(fieldChainText);
        result.FieldChain[0].ContainingType.ClrType.Should().Be(expectedType);
        result.FieldChain[0].Property.ReflectedType.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("CS", TBase, $"{DerivedQC}.{InfoDetail}", typeof(DerivedQ))]
    [InlineData("[OC]*S", TBase, $"{DerivedQC}.{InfoDetail}", typeof(DerivedQ))]
    [InlineData("[OC]*S", TBase, $"{BaseO}.{DerivedQC}.{InfoDetail}", typeof(DerivedQ))]
    [InlineData("CS", TDerivedQ, $"{DerivedQC}.{InfoDetail}", typeof(DerivedQ))]
    public void MatchSucceedsThroughDerivedCompound(string patternText, string resourceTypeName, string fieldChainText, Type expectedType)
    {
        // Arrange
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);
        ResourceType resourceType = _resourceGraph.GetResourceType(resourceTypeName);

        // Act
        PatternMatchResult result = pattern.Match(fieldChainText, resourceType, FieldChainPatternMatchOptions.AllowDerivedTypes, _loggerFactory);

        // Assert
        result.FailureMessage.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();

        string chainText = string.Join('.', result.FieldChain);
        chainText.Should().Be(fieldChainText);

        result.FieldChain.Single(field => field.PublicName == DerivedQC).ContainingType.ClrType.Should().Be(expectedType);
    }

    [Theory]
    // Fields: not found
    [InlineData("F", TBase, $"^{X}", $"Field '{X}' does not exist on resource type '{TBase}' or any of its derived types.")]
    [InlineData("FF", TBase, $"{BaseO}.^{X}", $"Field '{X}' does not exist on resource type '{TBase}' or any of its derived types.")]
    [InlineData("FF", TDerivedQ, $"^{DerivedVA}", $"Field '{DerivedVA}' does not exist on resource type '{TDerivedQ}'.")]
    [InlineData("FF", TDerivedQ, $"^{DerivedVO}", $"Field '{DerivedVO}' does not exist on resource type '{TDerivedQ}'.")]
    [InlineData("FF", TDerivedQ, $"^{DerivedVM}", $"Field '{DerivedVM}' does not exist on resource type '{TDerivedQ}'.")]
    [InlineData("FF", TDerivedQ, $"{DerivedQM}.^{DerivedVA}", $"Field '{DerivedVA}' does not exist on resource type '{TDerivedQ}'.")]
    [InlineData("FF", TDerivedV, $"^{DerivedQA}", $"Field '{DerivedQA}' does not exist on resource type '{TDerivedV}'.")]
    [InlineData("FF", TDerivedV, $"^{DerivedQO}", $"Field '{DerivedQO}' does not exist on resource type '{TDerivedV}'.")]
    [InlineData("FF", TDerivedV, $"^{DerivedQM}", $"Field '{DerivedQM}' does not exist on resource type '{TDerivedV}'.")]
    [InlineData("FF", TDerivedV, $"{DerivedVM}.^{DerivedQA}", $"Field '{DerivedQA}' does not exist on resource type '{TDerivedV}'.")]
    // Fields: multiple derived types
    [InlineData("F", TBase, $"^{DerivedA}", $"Field '{DerivedA}' is defined on multiple types that derive from resource type '{TBase}'.")]
    [InlineData("FF", TBase, $"{BaseO}.^{DerivedA}", $"Field '{DerivedA}' is defined on multiple types that derive from resource type '{TBase}'.")]
    [InlineData("F", TBase, $"^{DerivedO}", $"Field '{DerivedO}' is defined on multiple types that derive from resource type '{TBase}'.")]
    [InlineData("FF", TBase, $"{BaseM}.^{DerivedO}", $"Field '{DerivedO}' is defined on multiple types that derive from resource type '{TBase}'.")]
    [InlineData("F", TBase, $"^{DerivedM}", $"Field '{DerivedM}' is defined on multiple types that derive from resource type '{TBase}'.")]
    [InlineData("FF", TBase, $"{BaseO}.^{DerivedM}", $"Field '{DerivedM}' is defined on multiple types that derive from resource type '{TBase}'.")]
    [InlineData("F", TBase, $"^{DerivedToOneOrMany}", $"Field '{DerivedToOneOrMany}' is defined on multiple types that derive from resource type '{TBase}'.")]
    [InlineData("FF", TBase, $"{BaseM}.^{DerivedToOneOrMany}",
        $"Field '{DerivedToOneOrMany}' is defined on multiple types that derive from resource type '{TBase}'.")]
    // Compound attributes: not found, derived-only compound is not visible from the other derived type
    [InlineData("F", TDerivedV, $"^{DerivedQC}", $"Field '{DerivedQC}' does not exist on resource type '{TDerivedV}'.")]
    [InlineData("F", TDerivedQ, $"^{DerivedVL}", $"Field '{DerivedVL}' does not exist on resource type '{TDerivedQ}'.")]
    // Compound attributes: multiple derived types define the same compound
    [InlineData("C", TBase, $"^{SharedC}", $"Field '{SharedC}' is defined on multiple types that derive from resource type '{TBase}'.")]
    [InlineData("F", TBase, $"^{SharedC}", $"Field '{SharedC}' is defined on multiple types that derive from resource type '{TBase}'.")]
    [InlineData("FF", TBase, $"{BaseO}.^{SharedC}", $"Field '{SharedC}' is defined on multiple types that derive from resource type '{TBase}'.")]
    // Compound attributes: derived lookup is not applied inside a compound container
    [InlineData("CS", TBase, $"{DerivedQC}.^{DerivedA}", "Field 'derivedValue' does not exist on compound type 'InfoCompound'.")]
    [InlineData("CS", TBase, $"{DerivedQC}.^{X}", "Field 'unknown' does not exist on compound type 'InfoCompound'.")]
    [InlineData("CS", TBase, $"{DerivedQC}.^{BaseO}", "Field 'singleItem' does not exist on compound type 'InfoCompound'.")]
    [InlineData("CS", TBase, $"{DerivedQC}.{InfoDetail}.^{X}", "End of field chain expected.")]
    // Compound attributes: type mismatch
    [InlineData("S", TBase, $"^{DerivedQC}", $"Scalar non-collection attribute on resource type '{TBase}' expected.")]
    public void MatchFails(string patternText, string resourceTypeName, string fieldChainText, string failureMessage)
    {
        // Arrange
        var fieldChainSource = new MarkedText(fieldChainText, '^');
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);
        ResourceType resourceType = _resourceGraph.GetResourceType(resourceTypeName);

        // Act
        PatternMatchResult result = pattern.Match(fieldChainSource.Text, resourceType, FieldChainPatternMatchOptions.AllowDerivedTypes, _loggerFactory);

        // Assert
        result.FailureMessage.Should().Be(failureMessage);
        result.FailurePosition.Should().Be(fieldChainSource.Position);
        result.FieldChain.Should().BeEmpty();
        result.IsSuccess.Should().BeFalse();
    }

    public void Dispose()
    {
        _loggerFactory.Dispose();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private abstract class Base : Identifiable<long>
    {
        [Attr]
        public string? Value { get; set; }

        [HasOne]
        public Base SingleItem { get; set; } = null!;

        [HasMany]
        public ISet<Base> SetOfItems { get; set; } = new HashSet<Base>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DerivedQ : Base
    {
        [Attr]
        public string? DerivedValue { get; set; }

        [Attr]
        public string? DerivedQValue { get; set; }

        [HasOne]
        public Base? DerivedSingleItem { get; set; }

        [HasOne]
        public DerivedQ? DerivedQSingleItem { get; set; }

        [HasMany]
        public ISet<Base> DerivedSetOfItems { get; set; } = new HashSet<Base>();

        [HasMany]
        public ISet<DerivedQ> DerivedQSetOfItems { get; set; } = new HashSet<DerivedQ>();

        [HasOne]
        public Base? DerivedToOneOrToMany { get; set; }

        [Attr(IsCompound = true)]
        public InfoCompound? DerivedQInfo { get; set; }

        [Attr(IsCompound = true)]
        public List<InfoCompound> DerivedQPhones { get; set; } = [];

        [Attr(IsCompound = true)]
        public InfoCompound? SharedInfo { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DerivedV : Base
    {
        [Attr]
        public string? DerivedValue { get; set; }

        [Attr]
        public string? DerivedVValue { get; set; }

        [HasOne]
        public Base? DerivedSingleItem { get; set; }

        [HasOne]
        public DerivedV? DerivedVSingleItem { get; set; }

        [HasMany]
        public ISet<Base> DerivedSetOfItems { get; set; } = new HashSet<Base>();

        [HasMany]
        public ISet<DerivedV> DerivedVSetOfItems { get; set; } = new HashSet<DerivedV>();

        [HasMany]
        public ISet<Base> DerivedToOneOrToMany { get; set; } = new HashSet<Base>();

        [Attr]
        public List<string> DerivedVTags { get; set; } = [];

        [Attr(IsCompound = true)]
        public InfoCompound? SharedInfo { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class InfoCompound
    {
        [Attr]
        public string? Detail { get; set; }
    }
}
