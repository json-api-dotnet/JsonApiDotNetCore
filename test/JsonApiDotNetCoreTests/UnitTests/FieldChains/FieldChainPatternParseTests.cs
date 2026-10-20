using FluentAssertions;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using TestBuildingBlocks;
using Xunit;

// ReSharper disable StringLiteralTypo

namespace JsonApiDotNetCoreTests.UnitTests.FieldChains;

public sealed class FieldChainPatternParseTests
{
    [Theory]
    // Single segment with quantifier
    [InlineData("A?", "an optional attribute")]
    [InlineData("R+", "one or more relationships")]
    [InlineData("M*", "zero or more to-many relationships")]
    [InlineData("[OC]?", "an optional to-one relationship or compound non-collection attribute")]
    // Two segments
    [InlineData("MO", "a to-many relationship, followed by a to-one relationship")]
    [InlineData("MO+", "a to-many relationship, followed by one or more to-one relationships")]
    [InlineData("MO?", "a to-many relationship, followed by an optional to-one relationship")]
    [InlineData("M*O", "zero or more to-many relationships, followed by a to-one relationship")]
    [InlineData("R*M", "zero or more relationships, followed by a to-many relationship")]
    [InlineData("M*S", "zero or more to-many relationships, followed by a scalar non-collection attribute")]
    [InlineData("C+A", "one or more compound non-collection attributes, followed by an attribute")]
    [InlineData("[OA]?A", "an optional to-one relationship or attribute, followed by an attribute")]
    [InlineData("O*[MA]", "zero or more to-one relationships, followed by a to-many relationship or an attribute")]
    [InlineData("[RC]*S", "zero or more relationships or compound non-collection attributes, followed by a scalar non-collection attribute")]
    [InlineData("[MC]*[MSC]",
        "zero or more to-many relationships or compound non-collection attributes, followed by a to-many relationship or a non-collection attribute")]
    [InlineData("[RC]*[OLQ]",
        "zero or more relationships or compound non-collection attributes, followed by a to-one relationship or a collection attribute")]
    [InlineData("[CQ]*A", "zero or more compound attributes, followed by an attribute")]
    // Three segments
    [InlineData("O?M+A", "an optional to-one relationship, followed by one or more to-many relationships, followed by an attribute")]
    public void ParseSucceeds(string patternText, string displayText)
    {
        // Act
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);

        // Assert
        pattern.ToString().Should().Be(patternText);
        pattern.GetDescription().Should().Be(displayText);
    }

    [Theory]
    [InlineData("^", "Pattern is empty.")]
    [InlineData("^ ", "Unknown token ' '.")]
    [InlineData("^X", "Unknown token 'X'.")]
    [InlineData("A^X", "Unknown token 'X'.")]
    [InlineData("^*", "Field type or [ expected.")]
    [InlineData("^]", "Field type or [ expected.")]
    [InlineData("A^]", "Field type or [ expected.")]
    [InlineData("A+^+", "Field type or [ expected.")]
    [InlineData("[^", "Field type expected.")]
    [InlineData("[^?", "Field type expected.")]
    [InlineData("[^]", "Field type expected.")]
    [InlineData("[^[", "Field type expected.")]
    [InlineData("[A^", "Field type or ] expected.")]
    [InlineData("[A^+", "Field type or ] expected.")]
    [InlineData("[A^[", "Field type or ] expected.")]
    public void ParseFails(string patternText, string errorMessage)
    {
        // Arrange
        var patternSource = new MarkedText(patternText, '^');

        // Act
        Action action = () => FieldChainPattern.Parse(patternSource.Text);

        // Assert
        PatternFormatException exception = action.Should().ThrowExactly<PatternFormatException>().Which;
        exception.Message.Should().Be(errorMessage);
        exception.Position.Should().Be(patternSource.Position);
        exception.Pattern.Should().Be(patternSource.Text);
    }

    [Theory]
    [InlineData("O", "O", "a to-one relationship")]
    [InlineData("[O]", "O", "a to-one relationship")]
    [InlineData("M", "M", "a to-many relationship")]
    [InlineData("[M]", "M", "a to-many relationship")]
    [InlineData("R", "R", "a relationship")]
    [InlineData("[R]", "R", "a relationship")]
    [InlineData("S", "S", "a scalar non-collection attribute")]
    [InlineData("[S]", "S", "a scalar non-collection attribute")]
    [InlineData("C", "C", "a compound non-collection attribute")]
    [InlineData("[C]", "C", "a compound non-collection attribute")]
    [InlineData("L", "L", "a scalar collection attribute")]
    [InlineData("[L]", "L", "a scalar collection attribute")]
    [InlineData("Q", "Q", "a compound collection attribute")]
    [InlineData("[Q]", "Q", "a compound collection attribute")]
    [InlineData("A", "A", "an attribute")]
    [InlineData("[A]", "A", "an attribute")]
    [InlineData("F", "F", "a field")]
    [InlineData("[F]", "F", "a field")]
    public void ParseSingleChoice(string patternText, string reducedText, string displayText)
    {
        // Act
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);

        // Assert
        pattern.ToString().Should().Be(reducedText);
        pattern.GetDescription().Should().Be(displayText);
    }

    [Theory]
    // 2 tokens (10 combinations)
    [InlineData("[SO]", "[OS]", "a to-one relationship or a scalar non-collection attribute")]
    [InlineData("[CO]", "[OC]", "a to-one relationship or a compound non-collection attribute")]
    [InlineData("[LO]", "[OL]", "a to-one relationship or a scalar collection attribute")]
    [InlineData("[QO]", "[OQ]", "a to-one relationship or a compound collection attribute")]
    [InlineData("[SM]", "[MS]", "a to-many relationship or a scalar non-collection attribute")]
    [InlineData("[CM]", "[MC]", "a to-many relationship or a compound non-collection attribute")]
    [InlineData("[LM]", "[ML]", "a to-many relationship or a scalar collection attribute")]
    [InlineData("[QM]", "[MQ]", "a to-many relationship or a compound collection attribute")]
    [InlineData("[QS]", "[SQ]", "a scalar non-collection attribute or a compound collection attribute")]
    [InlineData("[CL]", "[LC]", "a scalar collection attribute or a compound non-collection attribute")]
    // 3 tokens (20 combinations)
    [InlineData("[SMO]", "[RS]", "a relationship or a scalar non-collection attribute")]
    [InlineData("[CMO]", "[RC]", "a relationship or a compound non-collection attribute")]
    [InlineData("[LMO]", "[RL]", "a relationship or a scalar collection attribute")]
    [InlineData("[QMO]", "[RQ]", "a relationship or a compound collection attribute")]
    [InlineData("[CSO]", "[OSC]", "a to-one relationship or a non-collection attribute")]
    [InlineData("[LSO]", "[OSL]", "a to-one relationship or a scalar attribute")]
    [InlineData("[QSO]", "[OSQ]", "a to-one relationship or a scalar non-collection attribute or a compound collection attribute")]
    [InlineData("[CLO]", "[OLC]", "a to-one relationship or a scalar collection attribute or a compound non-collection attribute")]
    [InlineData("[QCO]", "[OCQ]", "a to-one relationship or a compound attribute")]
    [InlineData("[QLO]", "[OLQ]", "a to-one relationship or a collection attribute")]
    [InlineData("[CSM]", "[MSC]", "a to-many relationship or a non-collection attribute")]
    [InlineData("[LSM]", "[MSL]", "a to-many relationship or a scalar attribute")]
    [InlineData("[QSM]", "[MSQ]", "a to-many relationship or a scalar non-collection attribute or a compound collection attribute")]
    [InlineData("[CLM]", "[MLC]", "a to-many relationship or a scalar collection attribute or a compound non-collection attribute")]
    [InlineData("[QCM]", "[MCQ]", "a to-many relationship or a compound attribute")]
    [InlineData("[QLM]", "[MLQ]", "a to-many relationship or a collection attribute")]
    [InlineData("[CLS]", "[SLC]", "a scalar attribute or a compound non-collection attribute")]
    [InlineData("[QCS]", "[SCQ]", "a scalar non-collection attribute or a compound attribute")]
    [InlineData("[QLS]", "[SLQ]", "a scalar attribute or a compound collection attribute")]
    [InlineData("[QCL]", "[LCQ]", "a scalar collection attribute or a compound attribute")]
    // 4 tokens (14 combinations)
    [InlineData("[CSMO]", "[RSC]", "a relationship or a non-collection attribute")]
    [InlineData("[LSMO]", "[RSL]", "a relationship or a scalar attribute")]
    [InlineData("[QSMO]", "[RSQ]", "a relationship or a scalar non-collection attribute or a compound collection attribute")]
    [InlineData("[CLMO]", "[RLC]", "a relationship or a scalar collection attribute or a compound non-collection attribute")]
    [InlineData("[QCMO]", "[RCQ]", "a relationship or a compound attribute")]
    [InlineData("[QLMO]", "[RLQ]", "a relationship or a collection attribute")]
    [InlineData("[CLSO]", "[OSLC]", "a to-one relationship or a scalar attribute or a compound non-collection attribute")]
    [InlineData("[QCSO]", "[OSCQ]", "a to-one relationship or a scalar non-collection attribute or a compound attribute")]
    [InlineData("[QLSO]", "[OSLQ]", "a to-one relationship or a scalar attribute or a compound collection attribute")]
    [InlineData("[QCLO]", "[OLCQ]", "a to-one relationship or a scalar collection attribute or a compound attribute")]
    [InlineData("[CLSM]", "[MSLC]", "a to-many relationship or a scalar attribute or a compound non-collection attribute")]
    [InlineData("[QCSM]", "[MSCQ]", "a to-many relationship or a scalar non-collection attribute or a compound attribute")]
    [InlineData("[QLSM]", "[MSLQ]", "a to-many relationship or a scalar attribute or a compound collection attribute")]
    [InlineData("[QCLM]", "[MLCQ]", "a to-many relationship or a scalar collection attribute or a compound attribute")]
    // 5 tokens (6 combinations)
    [InlineData("[CLSMO]", "[RSLC]", "a relationship or a scalar attribute or a compound non-collection attribute")]
    [InlineData("[QCSMO]", "[RSCQ]", "a relationship or a scalar non-collection attribute or a compound attribute")]
    [InlineData("[QLSMO]", "[RSLQ]", "a relationship or a scalar attribute or a compound collection attribute")]
    [InlineData("[QCLMO]", "[RLCQ]", "a relationship or a scalar collection attribute or a compound attribute")]
    [InlineData("[QCLSO]", "[OA]", "a to-one relationship or an attribute")]
    [InlineData("[QCLSM]", "[MA]", "a to-many relationship or an attribute")]
    public void ParseCombinesChoices(string patternText, string reducedText, string displayText)
    {
        // Act
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);

        // Assert
        pattern.ToString().Should().Be(reducedText);
        pattern.GetDescription().Should().Be(displayText);
    }

    [Theory]
    // Unification to attribute
    [InlineData("[SCLQ]", "A", "an attribute")]
    [InlineData("[SQLC]", "A", "an attribute")]
    [InlineData("[ASCLQ]", "A", "an attribute")]
    // Unification to relationship
    [InlineData("[MO]", "R", "a relationship")]
    [InlineData("[OM]", "R", "a relationship")]
    [InlineData("[MOR]", "R", "a relationship")]
    // Unification to field
    [InlineData("[MOSCLQ]", "F", "a field")]
    [InlineData("[RA]", "F", "a field")]
    [InlineData("[MOA]", "F", "a field")]
    [InlineData("[RSCLQ]", "F", "a field")]
    [InlineData("[MORSCLQ]", "F", "a field")]
    [InlineData("[MORACLQ]", "F", "a field")]
    [InlineData("[MORAF]", "F", "a field")]
    // Unification to scalar attribute
    [InlineData("[SL]", "[SL]", "a scalar attribute")]
    [InlineData("[LS]", "[SL]", "a scalar attribute")]
    // Unification to compound attribute
    [InlineData("[CQ]", "[CQ]", "a compound attribute")]
    [InlineData("[QC]", "[CQ]", "a compound attribute")]
    // Unification to non-collection attribute
    [InlineData("[SC]", "[SC]", "a non-collection attribute")]
    [InlineData("[CS]", "[SC]", "a non-collection attribute")]
    // Unification to collection attribute
    [InlineData("[LQ]", "[LQ]", "a collection attribute")]
    [InlineData("[QL]", "[LQ]", "a collection attribute")]
    public void ParseNormalizesChoices(string patternText, string reducedText, string displayText)
    {
        // Act
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);

        // Assert
        pattern.ToString().Should().Be(reducedText);
        pattern.GetDescription().Should().Be(displayText);
    }
}
