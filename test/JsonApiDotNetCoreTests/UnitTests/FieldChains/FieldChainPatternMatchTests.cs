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

// Workaround for Resharper bug at https://youtrack.jetbrains.com/issue/RSRP-494909/Breaking-UsedImplicitly-and-PublicAPI-on-types-no-longer-respected.
// ReSharper disable PropertyCanBeMadeInitOnly.Local

// ReSharper disable InconsistentNaming
#pragma warning disable AV1706 // Identifier contains an abbreviation or is too short

namespace JsonApiDotNetCoreTests.UnitTests.FieldChains;

public sealed class FieldChainPatternMatchTests
{
    private const string T = "resources";
    private const string X = "unknown";
    private const string A = "name";
    private const string O = "parent";
    private const string M = "children";

    private readonly LoggerFactory _loggerFactory;
    private readonly ResourceType _resourceType;

    public FieldChainPatternMatchTests(ITestOutputHelper testOutputHelper)
    {
        var loggerProvider = new XUnitLoggerProvider(testOutputHelper, null, LogOutputFields.Message);
        _loggerFactory = new LoggerFactory([loggerProvider]);

        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Resource, long>().Build();
        _resourceType = resourceGraph.GetResourceType<Resource>();
    }

    [Theory]
    // Field types
    [InlineData("M", M)]
    [InlineData("O", O)]
    [InlineData("R", O)]
    [InlineData("R", M)]
    [InlineData("A", A)]
    [InlineData("F", A)]
    [InlineData("F", O)]
    [InlineData("F", M)]
    // Field sets
    [InlineData("[MO]", O)]
    [InlineData("[MO]", M)]
    [InlineData("[OA]", O)]
    [InlineData("[OA]", A)]
    // Quantifiers
    [InlineData("M?", "")]
    [InlineData("M?", M)]
    [InlineData("M*", "")]
    [InlineData("M*", M)]
    [InlineData("M*", $"{M}.{M}")]
    [InlineData("M*", $"{M}.{M}.{M}.{M}.{M}")]
    [InlineData("M+", M)]
    [InlineData("M+", $"{M}.{M}")]
    [InlineData("M+", $"{M}.{M}.{M}.{M}.{M}")]
    // Quantifiers at start
    [InlineData("M*O", O)]
    [InlineData("M*O", $"{M}.{O}")]
    [InlineData("M*O", $"{M}.{M}.{O}")]
    [InlineData("M*O", $"{M}.{M}.{M}.{M}.{M}.{O}")]
    [InlineData("M?O", O)]
    [InlineData("M?O", $"{M}.{O}")]
    [InlineData("M+O", $"{M}.{O}")]
    [InlineData("M+O", $"{M}.{M}.{O}")]
    [InlineData("M+O", $"{M}.{M}.{M}.{M}.{M}.{O}")]
    // Quantifiers at start and at end
    [InlineData("M*OA*", O)]
    [InlineData("M*OA*", $"{M}.{O}")]
    [InlineData("M*OA*", $"{O}.{A}")]
    [InlineData("M*OA*", $"{M}.{O}.{A}")]
    [InlineData("M*OA*", $"{M}.{M}.{O}")]
    [InlineData("M*OA*", $"{M}.{M}.{O}.{A}")]
    [InlineData("M?OA?", O)]
    [InlineData("M?OA?", $"{M}.{O}")]
    [InlineData("M?OA?", $"{O}.{A}")]
    [InlineData("M?OA?", $"{M}.{O}.{A}")]
    [InlineData("M+OA+", $"{M}.{O}.{A}")]
    [InlineData("M+OA+", $"{M}.{M}.{O}.{A}")]
    // Field chains
    [InlineData("MA", $"{M}.{A}")]
    [InlineData("MOA", $"{M}.{O}.{A}")]
    // Backtracking multiple times
    [InlineData("F*M+O+", $"{M}.{M}.{M}.{M}.{M}.{O}.{O}.{O}.{O}.{O}")]
    [InlineData("F+OF*A?", $"{M}.{M}.{M}.{O}.{M}.{M}.{M}.{A}")]
    public void MatchSucceeds(string patternText, string fieldChainText)
    {
        // Arrange
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);

        // Act
        PatternMatchResult result = pattern.Match(fieldChainText, _resourceType, FieldChainPatternMatchOptions.None, _loggerFactory);

        // Assert
        result.FailureMessage.Should().BeEmpty();
        result.FailurePosition.Should().Be(-1);
        result.FieldChain.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        string chainText = string.Join('.', result.FieldChain);
        chainText.Should().Be(fieldChainText);
    }

    [Theory]
    // Invalid field chain
    [InlineData("MOA", "^ ", "Field name expected.")]
    [InlineData("MOA", $"^ {M}", "Field name expected.")]
    [InlineData("MOA", $"^{M} ", "Field name expected.")]
    [InlineData("MOA", $"^ {M} ", "Field name expected.")]
    [InlineData("MOA", "^.", "Field name expected.")]
    [InlineData("MOA", $"{M}.^.{M}", "Field name expected.")]
    [InlineData("MOA", $"{M}.^", "Field name expected.")]
    [InlineData("MOA", $"{M}.^ {M}", "Field name expected.")]
    [InlineData("MOA", $"^.{M}", "Field name expected.")]
    [InlineData("MOA", $"^.{M}.", "Field name expected.")]
    [InlineData("MOA", $"{M}.^ .{O}", "Field name expected.")]
    // Fields: insufficient input
    [InlineData("M", "^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MM", $"{M}^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM", $"{M}.{M}^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("O", "^", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OO", $"{O}^", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO", $"{O}.{O}^", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("R", "^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("RR", $"{M}^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("RRR", $"{M}.{O}^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("A", "^", $"Attribute on resource type '{T}' expected.")]
    [InlineData("AA", $"{A}^", "Attribute expected.")]
    [InlineData("F", "^", $"Field on resource type '{T}' expected.")]
    [InlineData("FF", $"{M}^", $"Field on resource type '{T}' expected.")]
    [InlineData("FFF", $"{O}.{M}^", $"Field on resource type '{T}' expected.")]
    [InlineData("MOA", "^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MOA", $"{M}^", $"To-one relationship on resource type '{T}' expected.")]
    // Fields: too much input
    [InlineData("A", $"{A}.^{X}", "End of field chain expected.")]
    [InlineData("OO", $"{O}.{O}.^{O}", "End of field chain expected.")]
    [InlineData("MMM", $"{M}.{M}.{M}.^{A}", "End of field chain expected.")]
    // Fields: not found
    [InlineData("M", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("MM", $"{M}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("MMM", $"{M}.{M}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("O", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("OO", $"{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("OOO", $"{O}.{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("R", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("RR", $"{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("RRR", $"{M}.{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("A", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("AA", $"{A}.^{X}", $"Field '{X}' does not exist.")]
    [InlineData("AAA", $"{A}.^{A}.{X}", $"Field '{A}' does not exist.")]
    [InlineData("F", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("FF", $"{M}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("FFF", $"{O}.{M}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    // Fields: incompatible type
    [InlineData("M", $"^{O}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("M", $"^{A}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MM", $"{M}.^{O}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MM", $"{M}.^{A}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM", $"{M}.{M}.^{O}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM", $"{M}.{M}.^{A}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("O", $"^{M}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("O", $"^{A}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OO", $"{O}.^{M}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OO", $"{O}.^{A}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO", $"{O}.{O}.^{M}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO", $"{O}.{O}.^{A}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("R", $"^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("RR", $"{O}.^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("RRR", $"{O}.{O}.^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("A", $"^{M}", $"Attribute on resource type '{T}' expected.")]
    [InlineData("A", $"^{O}", $"Attribute on resource type '{T}' expected.")]
    // Field sets: insufficient input
    [InlineData("[AR]", "^", $"Field on resource type '{T}' expected.")]
    [InlineData("[MO][AR]", $"{M}^", $"Field on resource type '{T}' expected.")]
    [InlineData("[AR][AR]", $"{A}^", "Field expected.")]
    [InlineData("[MO][MO][AR]", $"{O}.{M}^", $"Field on resource type '{T}' expected.")]
    [InlineData("[MA]", "^", $"To-many relationship or attribute on resource type '{T}' expected.")]
    [InlineData("[MO][MO]", $"{M}^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("[MO][MO][MA]", $"{M}.{O}^", $"To-many relationship or attribute on resource type '{T}' expected.")]
    [InlineData("[MO][MO][OA]", $"{O}.{M}^", $"To-one relationship or attribute on resource type '{T}' expected.")]
    // Field sets: too much input
    [InlineData("[AM]", $"{A}.^{X}", "End of field chain expected.")]
    [InlineData("[MO][MO]", $"{O}.{M}.^{O}", "End of field chain expected.")]
    [InlineData("[MO][MO][MA]", $"{M}.{O}.{M}.^{A}", "End of field chain expected.")]
    // Field sets: not found
    [InlineData("[MOA]", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("[MOA][MOA]", $"{M}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("[MOA][MOA][MOA]", $"{M}.{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    // Field sets: incompatible type
    [InlineData("[MO]", $"^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("[MO][MO]", $"{M}.^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("[MO][MO][MO]", $"{M}.{O}.^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("[OA]", $"^{M}", $"To-one relationship or attribute on resource type '{T}' expected.")]
    [InlineData("[MA]", $"^{O}", $"To-many relationship or attribute on resource type '{T}' expected.")]
    // Quantifiers: insufficient input
    [InlineData("M+", "^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MM+", $"{M}^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM+", $"{M}.{M}^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("O+", "^", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OO+", $"{O}^", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO+", $"{O}.{O}^", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("R+", "^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("RR+", $"{M}^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("RRR+", $"{M}.{O}^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("A+", "^", $"Attribute on resource type '{T}' expected.")]
    [InlineData("AA+", $"{A}^", "Attribute expected.")]
    [InlineData("F+", "^", $"Field on resource type '{T}' expected.")]
    [InlineData("FF+", $"{M}^", $"Field on resource type '{T}' expected.")]
    [InlineData("FFF+", $"{O}.{M}^", $"Field on resource type '{T}' expected.")]
    [InlineData("M*O*A", "^", $"Field on resource type '{T}' expected.")]
    [InlineData("M*O*A", $"{M}.{O}^", $"To-one relationship or attribute on resource type '{T}' expected.")]
    [InlineData("M*O*A", $"{M}.{M}.{O}.{O}^", $"To-one relationship or attribute on resource type '{T}' expected.")]
    [InlineData("M?O?A", "^", $"Field on resource type '{T}' expected.")]
    [InlineData("M?O?A", $"{M}.{O}^", $"Attribute on resource type '{T}' expected.")]
    [InlineData("M+O+A", "^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("M+O+A", $"{M}.{O}^", $"To-one relationship or attribute on resource type '{T}' expected.")]
    [InlineData("M+O+A", $"{M}.{M}.{O}.{O}^", $"To-one relationship or attribute on resource type '{T}' expected.")]
    // Quantifiers: too much input
    [InlineData("M*", $"{M}.^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MM*", $"{M}.{M}.^{O}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM*", $"{M}.{M}.{M}.^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("O*", $"{O}.^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OO*", $"{O}.{O}.^{M}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO*", $"{O}.{O}.{O}.^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("R*", $"{M}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("RR*", $"{M}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("RRR*", $"{O}.{M}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("[MO]*", $"{M}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("[MO][MO]*", $"{M}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("[MO][MO][MO]*", $"{O}.{M}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("M?", $"{M}.^{M}", "End of field chain expected.")]
    [InlineData("M?", $"{M}.^{A}", "End of field chain expected.")]
    [InlineData("MM?", $"{M}.{M}.^{M}", "End of field chain expected.")]
    [InlineData("MM?", $"{M}.{M}.^{O}", "End of field chain expected.")]
    [InlineData("MMM?", $"{M}.{M}.{M}.^{M}", "End of field chain expected.")]
    [InlineData("MMM?", $"{M}.{M}.{M}.^{A}", "End of field chain expected.")]
    [InlineData("O?", $"{O}.^{O}", "End of field chain expected.")]
    [InlineData("O?", $"{O}.^{A}", "End of field chain expected.")]
    [InlineData("OO?", $"{O}.{O}.^{O}", "End of field chain expected.")]
    [InlineData("OO?", $"{O}.{O}.^{M}", "End of field chain expected.")]
    [InlineData("OOO?", $"{O}.{O}.{O}.^{O}", "End of field chain expected.")]
    [InlineData("OOO?", $"{O}.{O}.{O}.^{A}", "End of field chain expected.")]
    [InlineData("R?", $"{O}.^{O}", "End of field chain expected.")]
    [InlineData("R?", $"{O}.^{A}", "End of field chain expected.")]
    [InlineData("RR?", $"{O}.{O}.^{O}", "End of field chain expected.")]
    [InlineData("RR?", $"{O}.{O}.^{A}", "End of field chain expected.")]
    [InlineData("RRR?", $"{O}.{O}.{O}.^{O}", "End of field chain expected.")]
    [InlineData("RRR?", $"{O}.{O}.{O}.^{A}", "End of field chain expected.")]
    [InlineData("F?", $"{O}.^{O}", "End of field chain expected.")]
    [InlineData("F?", $"{O}.^{A}", "End of field chain expected.")]
    [InlineData("FF?", $"{O}.{O}.^{O}", "End of field chain expected.")]
    [InlineData("FF?", $"{O}.{O}.^{A}", "End of field chain expected.")]
    [InlineData("FFF?", $"{O}.{O}.{O}.^{O}", "End of field chain expected.")]
    [InlineData("FFF?", $"{O}.{O}.{O}.^{A}", "End of field chain expected.")]
    [InlineData("M+", $"{M}.^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MM+", $"{M}.{M}.^{O}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM+", $"{M}.{M}.{M}.^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("O+", $"{O}.^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OO+", $"{O}.{O}.^{M}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO+", $"{O}.{O}.{O}.^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("R+", $"{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("RR+", $"{M}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("RRR+", $"{O}.{M}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("[MO]+", $"{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("[MO][MO]+", $"{M}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("[MO][MO][MO]+", $"{O}.{M}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("M[OA]+", $"{M}.{O}.^{M}", $"End of field chain or to-one relationship or attribute on resource type '{T}' expected.")]
    // Quantifiers: not found
    [InlineData("M*", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("MM*", $"{M}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("MMM*", $"{M}.{M}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("O*", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("OO*", $"{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("OOO*", $"{O}.{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("R?", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("RR?", $"{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("RRR?", $"{M}.{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("[AM]+", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("[AM][AM]+", $"{A}.^{X}", $"Field '{X}' does not exist.")]
    [InlineData("[AM][AM][AM]+", $"{A}.^{A}.{X}", $"Field '{A}' does not exist.")]
    [InlineData("F+", $"^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("FF+", $"{M}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("FFF+", $"{O}.{M}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    // Quantifiers: incompatible type
    [InlineData("M*", $"^{O}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("M*", $"^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MM*", $"{M}.^{O}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MM*", $"{M}.^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM*", $"{M}.{M}.^{O}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM*", $"{M}.{M}.^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("M?", $"^{O}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("M?", $"^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MM?", $"{M}.^{O}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MM?", $"{M}.^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM?", $"{M}.{M}.^{O}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM?", $"{M}.{M}.^{A}", $"End of field chain or to-many relationship on resource type '{T}' expected.")]
    [InlineData("M+", $"^{O}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("M+", $"^{A}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MM+", $"{M}.^{O}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MM+", $"{M}.^{A}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM+", $"{M}.{M}.^{O}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("MMM+", $"{M}.{M}.^{A}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("O*", $"^{M}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("O*", $"^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OO*", $"{O}.^{M}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OO*", $"{O}.^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO*", $"{O}.{O}.^{M}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO*", $"{O}.{O}.^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("O?", $"^{M}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("O?", $"^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OO?", $"{O}.^{M}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OO?", $"{O}.^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO?", $"{O}.{O}.^{M}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO?", $"{O}.{O}.^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("O+", $"^{M}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("O+", $"^{A}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OO+", $"{O}.^{M}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OO+", $"{O}.^{A}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO+", $"{O}.{O}.^{M}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("OOO+", $"{O}.{O}.^{A}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("[OM]*", $"^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("RR*", $"{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("RRR*", $"{O}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("R?", $"^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("[OM][OM]?", $"{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("RRR?", $"{O}.{O}.^{A}", $"End of field chain or relationship on resource type '{T}' expected.")]
    [InlineData("R+", $"^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("RR+", $"{O}.^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("[OM][OM][OM]+", $"{O}.{O}.^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("A*", $"^{M}", $"End of field chain or attribute on resource type '{T}' expected.")]
    [InlineData("A*", $"^{O}", $"End of field chain or attribute on resource type '{T}' expected.")]
    [InlineData("A?", $"^{M}", $"End of field chain or attribute on resource type '{T}' expected.")]
    [InlineData("A?", $"^{O}", $"End of field chain or attribute on resource type '{T}' expected.")]
    [InlineData("A+", $"^{M}", $"Attribute on resource type '{T}' expected.")]
    [InlineData("A+", $"^{O}", $"Attribute on resource type '{T}' expected.")]
    [InlineData("[MA]*", $"^{O}", $"End of field chain or to-many relationship or attribute on resource type '{T}' expected.")]
    [InlineData("[OA]*", $"^{M}", $"End of field chain or to-one relationship or attribute on resource type '{T}' expected.")]
    [InlineData("M+A", $"{M}.^{O}", $"To-many relationship or attribute on resource type '{T}' expected.")]
    [InlineData("M+A?", $"{M}.^{O}", $"End of field chain or to-many relationship or attribute on resource type '{T}' expected.")]
    // Quantifiers at start
    [InlineData("M*A", "^", $"To-many relationship or attribute on resource type '{T}' expected.")]
    [InlineData("M*A", $"{A}.^{O}", "End of field chain expected.")]
    [InlineData("M*O", "^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("M*O", $"^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("M*O", $"{M}.^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("M?A", "^", $"To-many relationship or attribute on resource type '{T}' expected.")]
    [InlineData("M?A", $"{A}.^{O}", "End of field chain expected.")]
    [InlineData("M?O", "^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("M?O", $"^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("M?O", $"{M}^", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("M?O", $"{M}.^{A}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("M+O", "^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("M+O", $"{M}.^{A}", $"Relationship on resource type '{T}' expected.")]
    [InlineData("M+OA", $"{M}^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("O+A", $"{O}.^{M}", $"To-one relationship or attribute on resource type '{T}' expected.")]
    [InlineData("[MA]+O", $"{M}^", $"Field on resource type '{T}' expected.")]
    // Quantifiers at start and at end
    [InlineData("M*OA*", "^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("M*OA*", $"{O}.{A}.^{A}", $"Field '{A}' does not exist.")]
    [InlineData("M*OA*", $"{M}.{M}.{O}.{A}.^{A}", $"Field '{A}' does not exist.")]
    [InlineData("M*OA*", $"^{X}.{O}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("M*OA*", $"{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("M?OA?", "^", $"Relationship on resource type '{T}' expected.")]
    [InlineData("M?OA?", $"^{X}.{O}.{A}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("M?OA?", $"{M}.^{M}.{O}.{A}", $"To-one relationship on resource type '{T}' expected.")]
    [InlineData("M?OA?", $"{M}.{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("M?OA?", $"{M}.{O}.{A}.^{A}", "End of field chain expected.")]
    [InlineData("M+OA+", "^", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("M+OA+", $"{M}.{O}^", $"Attribute on resource type '{T}' expected.")]
    [InlineData("M+OA+", $"^{O}.{A}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("M+OA+", $"{M}.{M}.{O}.{A}.^{A}", $"Field '{A}' does not exist.")]
    [InlineData("M+OA+", $"^{O}", $"To-many relationship on resource type '{T}' expected.")]
    [InlineData("M+OA+", $"^{X}.{O}.{A}", $"Field '{X}' does not exist on resource type '{T}'.")]
    [InlineData("M+OA+", $"{M}.{O}.^{X}", $"Field '{X}' does not exist on resource type '{T}'.")]
    // Backtracking multiple times
    [InlineData("M+O+", $"{M}.{M}.{O}.{O}.^{A}", $"End of field chain or to-one relationship on resource type '{T}' expected.")]
    [InlineData("F*M+O+", $"{M}.{M}.{O}.{O}.{A}^", "Field expected.")]
    [InlineData("F+R*A", $"{M}.{M}.{M}.{O}.{M}.{M}.{M}.{O}^", $"Field on resource type '{T}' expected.")]
    public void MatchFails(string patternText, string fieldChainText, string failureMessage)
    {
        // Arrange
        var fieldChainSource = new MarkedText(fieldChainText, '^');
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);

        // Act
        PatternMatchResult result = pattern.Match(fieldChainSource.Text, _resourceType, FieldChainPatternMatchOptions.None, _loggerFactory);

        // Assert
        result.FailureMessage.Should().Be(failureMessage);
        result.FailurePosition.Should().Be(fieldChainSource.Position);
        result.FieldChain.Should().BeEmpty();
        result.IsSuccess.Should().BeFalse();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Resource : Identifiable<long>
    {
        [Attr]
        public string? Name { get; set; }

        [HasOne]
        public Resource? Parent { get; set; }

        [HasMany]
        public ISet<Resource> Children { get; set; } = new HashSet<Resource>();
    }
}
