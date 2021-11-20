using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

internal sealed class ConcurrencyFakers : FakerContainer
{
    private readonly Lazy<Faker<WebPage>> _lazyWebPageFaker = new(() =>
        new Faker<WebPage>()
            .UseSeed(GetFakerSeed())
            .RuleFor(webPage => webPage.Title, faker => faker.Lorem.Sentence()));

    private readonly Lazy<Faker<FriendlyUrl>> _lazyFriendlyUrlFaker = new(() =>
        new Faker<FriendlyUrl>()
            .UseSeed(GetFakerSeed())
            .RuleFor(friendlyUrl => friendlyUrl.Uri, faker => faker.Internet.Url()));

    private readonly Lazy<Faker<TextBlock>> _lazyTextBlockFaker = new(() =>
        new Faker<TextBlock>()
            .UseSeed(GetFakerSeed())
            .RuleFor(textBlock => textBlock.ColumnCount, faker => faker.Random.Int(1, 3)));

    private readonly Lazy<Faker<Paragraph>> _lazyParagraphFaker = new(() =>
        new Faker<Paragraph>()
            .UseSeed(GetFakerSeed())
            .RuleFor(paragraph => paragraph.Heading, faker => faker.Lorem.Sentence())
            .RuleFor(paragraph => paragraph.Text, faker => faker.Lorem.Paragraph()));

    private readonly Lazy<Faker<WebImage>> _lazyWebImageFaker = new(() =>
        new Faker<WebImage>()
            .UseSeed(GetFakerSeed())
            .RuleFor(webImage => webImage.Description, faker => faker.Lorem.Sentence())
            .RuleFor(webImage => webImage.Path, faker => faker.Image.PicsumUrl()));

    private readonly Lazy<Faker<PageFooter>> _lazyPageFooterFaker = new(() =>
        new Faker<PageFooter>()
            .UseSeed(GetFakerSeed())
            .RuleFor(pageFooter => pageFooter.Copyright, faker => faker.Lorem.Sentence()));

    private readonly Lazy<Faker<WebLink>> _lazyWebLinkFaker = new(() =>
        new Faker<WebLink>()
            .UseSeed(GetFakerSeed())
            .RuleFor(webLink => webLink.Text, faker => faker.Lorem.Word())
            .RuleFor(webLink => webLink.Url, faker => faker.Internet.Url())
            .RuleFor(webLink => webLink.OpensInNewTab, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<DeploymentJob>> _lazyDeploymentJobFaker = new(() =>
        new Faker<DeploymentJob>()
            .UseSeed(GetFakerSeed())
            .RuleFor(deploymentJob => deploymentJob.StartedAt, faker => faker.Date.PastOffset()));

    public Faker<WebPage> WebPage => _lazyWebPageFaker.Value;
    public Faker<FriendlyUrl> FriendlyUrl => _lazyFriendlyUrlFaker.Value;
    public Faker<TextBlock> TextBlock => _lazyTextBlockFaker.Value;
    public Faker<Paragraph> Paragraph => _lazyParagraphFaker.Value;
    public Faker<WebImage> WebImage => _lazyWebImageFaker.Value;
    public Faker<PageFooter> PageFooter => _lazyPageFooterFaker.Value;
    public Faker<WebLink> WebLink => _lazyWebLinkFaker.Value;
    public Faker<DeploymentJob> DeploymentJob => _lazyDeploymentJobFaker.Value;
}
