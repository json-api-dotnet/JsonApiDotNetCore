# JsonApiDotNetCore

A [json:api](https://jsonapi.org) web application framework for .NET Core.

## Objectives

### 1. Eliminate Boilerplate

The goal of this package is to facilitate the development of APIs that leverage the full range
of features provided by the json:api specification.

Eliminate CRUD boilerplate and provide the following features across your resource endpoints, from HTTP all the way down to the database:

- Filtering
- Sorting
- Pagination
- Sparse fieldset selection
- Relationship inclusion and navigation

Checkout the [example requests](request-examples/index.md) to see the kind of features you will get out of the box.

### 2. Extensibility

This library relies heavily on an open-generic-based dependency injection model, which allows for easy per-resource customization.

