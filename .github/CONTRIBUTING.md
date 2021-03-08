# I don't want to read this whole thing I just have a question!!!

> Note: Please don't file an issue to ask a question.

You'll get faster results by using our official [Gitter channel](https://gitter.im/json-api-dotnet-core/Lobby) or [StackOverflow](https://stackoverflow.com/search?q=jsonapidotnetcore) where the community chimes in with helpful advice if you have questions.

# How can I contribute?

## Reporting bugs

This section guides you through submitting a bug report.
Following these guidelines helps maintainers and the community understand your report, reproduce the behavior and find related reports.

Before creating bug reports:
- Perform a search to see if the problem has already been reported. If it has and the issue is still open, add a comment to the existing issue instead of opening a new one. If you find a Closed issue that seems like it is the same thing that you're experiencing, open a new issue and include a link to the original issue in the body of your new one.
- Clone the source and run the project locally. You might be able to find the cause of the problem and fix things yourself. Most importantly, check if you can reproduce the problem in the latest version of the master branch.

When you are creating a bug report, please include as many details as possible.
Fill out the issue template, the information it asks for helps us resolve issues faster.

### How do I submit a (good) bug report?

Bugs are tracked as [GitHub issues](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues). Create an issue and provide the following information by filling in the template.
Explain the problem and include additional details to help maintainers reproduce the problem:

- **Use a clear and descriptive title** for the issue to identify the problem.
- **Describe the exact steps which reproduce the problem** in as many details as possible. When listing steps, don't just say what you did, but explain how you did it. 
- **Provide specific examples to demonstrate the steps.** Include links to files or GitHub projects, or copy/pasteable snippets, which you use in those examples. If you're providing snippets in the issue, use [Markdown code blocks](https://docs.github.com/en/github/writing-on-github/creating-and-highlighting-code-blocks).
- **Describe the behavior you observed after following the steps** and point out what exactly is the problem with that behavior. Explain which behavior you expected to see instead and why.
- **If you're reporting a crash**, include the full exception stack trace.

## Suggesting enhancements

This section guides you through submitting an enhancement suggestion, including completely new features and minor improvements to existing functionality. Following these guidelines helps maintainers and the community understand your suggestion and find related suggestions.

Before creating enhancement suggestions:
- Check the [documentation](https://www.jsonapi.net/usage/resources/index.html) and [integration tests](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreExampleTests/IntegrationTests) for existing features. You might discover the enhancement is already available.
- Perform a search to see if the feature has already been reported. If it has and the issue is still open, add a comment to the existing issue instead of opening a new one.

When you are creating an enhancement suggestion, please include as many details as possible. Fill in the template, including the steps that you imagine you would take if the feature you're requesting existed.

- **Use a clear and descriptive title** for the issue to identify the suggestion.
- **Provide a step-by-step description of the suggested enhancement** in as many details as possible.
- **Provide specific examples to demonstrate the usage.** Include copy/pasteable snippets which you use in those examples, as [Markdown code blocks](https://docs.github.com/en/github/writing-on-github/creating-and-highlighting-code-blocks).
- **Describe the current behavior and explain which behavior you expected to see instead** and why.
- **Explain why this enhancement would be useful** to most users and isn't something that can or should be implemented in your API project directly.
- **Verify that your enhancement does not conflict** with the [JSON:API specification](https://jsonapi.org/).

## Your first code contribution

Unsure where to begin contributing? You can start by looking through these [beginner](https://github.com/json-api-dotnet/JsonApiDotNetCore/labels/good%20first%20issue) and [help-wanted](https://github.com/json-api-dotnet/JsonApiDotNetCore/labels/help%20wanted) issues.

## Pull requests

Please follow these steps to have your contribution considered by the maintainers:

- **The worst thing in the world is opening a PR that gets rejected** after you've put a lot of effort in it. So for any non-trivial changes, open an issue first to discuss your approach and ensure it fits the product vision.
- Follow all instructions in the template. Don't forget to add tests and update documentation.
- After you submit your pull request, verify that all status checks are passing. In release builds, all compiler warnings are treated as errors, so you should address them before push.

We use [CSharpGuidelines](https://csharpcodingguidelines.com/) as our coding standard (with a few minor exceptions). Coding style is validated during PR build, where we inject an extra settings layer that promotes various suggestions to warning level. This ensures a high-quality codebase without interfering too much when editing code.
You can run the following [PowerShell scripts](https://github.com/PowerShell/PowerShell/releases) locally:
- `inspectcode.ps1`: Scans the code for style violations and opens the result in your web browser.
- `cleanupcode.ps1` Reformats the entire codebase to match with our configured style.

Code inspection violations can be addressed in several ways, depending on the situation:
- Types that are reported to be never instantiated (because the IoC container creates them dynamically) should be decorated with `[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]`.
- Exposed models that contain members never being read/written/assigned to should be decorated with `[UsedImplicitly(ImplicitUseTargetFlags.Members)]`.
- Types that are part of our public API surface can be decorated with `[PublicAPI]`. This suppresses messages such as "type can be marked sealed/internal", "virtual member is never overridden", "member is never used" etc.
- Incorrect violations can be [suppressed](https://www.jetbrains.com/help/resharper/Code_Analysis__Code_Inspections.html#ids-of-code-inspections) using a code comment.

In few cases, the automatic reformat decreases the readability of code. For example, when calling a Fluent API using chained method calls. This can be prevented using [formatter directives](https://www.jetbrains.com/help/resharper/Enforcing_Code_Formatting_Rules.html#configure):

```c#
public sealed class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // @formatter:wrap_chained_method_calls chop_always

        builder.Entity<MusicTrack>()
            .HasOne(musicTrack => musicTrack.Lyric)
            .WithOne(lyric => lyric.Track)
            .HasForeignKey<MusicTrack>();

        // @formatter:wrap_chained_method_calls restore
    }
}
```

## Backporting and hotfixes (for maintainers)

- Checkout the version you want to apply the feature on top of and create a new branch to release the new version:
  ```
  git checkout tags/v2.5.1 -b release/2.5.2
  ```
- Cherrypick the merge commit: `git cherry-pick {git commit SHA}`
- Bump the package version in the csproj
- Make any other compatibility, documentation or tooling related changes
- Push the branch to origin and verify the build
- Once the build is verified, create a GitHub release, tagging the release branch
- Open a PR back to master with any other additions
