# Contributing

## Workflow

1.  Search through the issues to see if your particular issue has already been discovered and possibly addressed
2.  Open an issue if you can't find anything helpful
3.  Open a PR for proposed changes

## Commit Guidelines

I have chosen to loosely follow the [Angular Commit Guidelines](https://github.com/angular/angular.js/blob/master/CONTRIBUTING.md#commit)

# Documentation

If you'd like to help us improve our documentation, please checkout our [GitHub pages repository](https://github.com/json-api-dotnet/json-api-dotnet.github.io) where we host our documentation.

# Backporting Features To Prior Releases

To backport a feature that has been approved and merged into `master`:

## Requester

Open an issue requesting the feature you would like backported. The change will be reviewed based on: 

- compatibility
- difficulty in porting the change 
- the added value the feature provides for users on the older versions
- how difficult it is for users to migrate from the older version to the newer version

## Maintainer

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
