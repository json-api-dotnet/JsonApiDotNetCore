# Processing queries

_since v4.0_

The query pipeline roughly looks like this:

```
HTTP --[ASP.NET Core]--> QueryString --[JADNC:QueryStringParameterReader]--> QueryExpression[] --[JADNC:ResourceService]--> QueryLayer --[JADNC:Repository]--> IQueryable --[EF Core]--> SQL
```

Processing a request involves the following steps:
- `JsonApiMiddleware` collects resource info from routing data for the current request.
- `JsonApiReader` transforms json request body into objects.
- `JsonApiController` accepts get/post/patch/delete verb and delegates to service.
- `IQueryStringParameterReader`s delegate to `QueryParser`s that transform query string text into `QueryExpression` objects.
	- By using prefix notation in filters, we don't need users to remember operator precedence and associativity rules.
	- These validated expressions contain direct references to attributes and relationships.
	- The readers also implement `IQueryConstraintProvider`, which exposes expressions through `ExpressionInScope` objects.
- `QueryLayerComposer` (used from `JsonApiResourceService`) collects all query constraints.
	- It combines them with default options and `IResourceDefinition` overrides and composes a tree of `QueryLayer` objects.
	- It lifts the tree for nested endpoints like /blogs/1/articles and rewrites includes.
	- `JsonApiResourceService` contains no more usage of `IQueryable`.
- `EntityFrameworkCoreRepository` delegates to `QueryableBuilder` to transform the `QueryLayer` tree into `IQueryable` expression trees.
	`QueryBuilder` depends on `QueryClauseBuilder` implementations that visit the tree nodes, transforming them to `System.Linq.Expression` equivalents.
	The `IQueryable` expression trees are executed by EF Core, which produces SQL statements out of them.
- `JsonApiWriter` transforms resource objects into json response.

# Example
To get a sense of what this all looks like, let's look at an example query string:

```
/api/v1/blogs?
  include=owner,articles.revisions.author&
  filter=has(articles)&
  sort=count(articles)&
  page[number]=3&
  fields[blogs]=title&
    filter[articles]=and(not(equals(author.firstName,null)),has(revisions))&
    sort[articles]=author.lastName&
    fields[articles]=url&
      filter[articles.revisions]=and(greaterThan(publishTime,'2001-01-01'),startsWith(author.firstName,'J'))&
      sort[articles.revisions]=-publishTime,author.lastName&
      fields[revisions]=publishTime
```

After parsing, the set of scoped expressions is transformed into the following tree by `QueryLayerComposer`:

```
QueryLayer<Blog>
{
  Include: owner,articles.revisions
  Filter: has(articles)
  Sort: count(articles)
  Pagination: Page number: 3, size: 5
  Projection
  {
    title
    id
    owner: QueryLayer<Author>
    {
      Sort: id
      Pagination: Page number: 1, size: 5
    }
    articles: QueryLayer<Article>
    {
      Filter: and(not(equals(author.firstName,null)),has(revisions))
      Sort: author.lastName
      Pagination: Page number: 1, size: 5
      Projection
      {
        url
        id
        revisions: QueryLayer<Revision>
        {
          Filter: and(greaterThan(publishTime,'2001-01-01'),startsWith(author.firstName,'J'))
          Sort: -publishTime,author.lastName
          Pagination: Page number: 1, size: 5
          Projection
          {
            publishTime
            id
          }
        }
      }
    }
  }
}
```

Next, the repository translates this into a LINQ query that the following C# code would represent:

```c#
var query = dbContext.Blogs
    .Include("Owner")
    .Include("Articles.Revisions")
    .Where(blog => blog.Articles.Any())
    .OrderBy(blog => blog.Articles.Count)
    .Skip(10)
    .Take(5)
    .Select(blog => new Blog
    {
        Title = blog.Title,
        Id = blog.Id,
        Owner = blog.Owner,
        Articles = new List<Article>(blog.Articles
            .Where(article => article.Author.FirstName != null && article.Revisions.Any())
            .OrderBy(article => article.Author.LastName)
            .Take(5)
            .Select(article => new Article
            {
                Url = article.Url,
                Id = article.Id,
                Revisions = new HashSet<Revision>(article.Revisions
                    .Where(revision => revision.PublishTime > DateTime.Parse("2001-01-01") && revision.Author.FirstName.StartsWith("J"))
                    .OrderByDescending(revision => revision.PublishTime)
                    .ThenBy(revision => revision.Author.LastName)
                    .Take(5)
                    .Select(revision => new Revision
                    {
                        PublishTime = revision.PublishTime,
                        Id = revision.Id
                    }))
            }))
    });
```
