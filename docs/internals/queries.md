# Processing queries

_since v4.0_

The query pipeline roughly looks like this:

```
HTTP --[ASP.NET]--> QueryString --[JADNC:QueryStringParameterReader]--> QueryExpression[] --[JADNC:ResourceService]--> QueryLayer --[JADNC:Repository]--> IQueryable --[Entity Framework Core]--> SQL
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
	- It lifts the tree for secondary endpoints like /blogs/1/articles and rewrites includes.
	- `JsonApiResourceService` contains no more usage of `IQueryable`.
- `EntityFrameworkCoreRepository` delegates to `QueryableBuilder` to transform the `QueryLayer` tree into `IQueryable` expression trees.
	`QueryBuilder` depends on `QueryClauseBuilder` implementations that visit the tree nodes, transforming them to `System.Linq.Expression` equivalents.
	The `IQueryable` expression trees are passed to Entity Framework Core, which produces SQL statements out of them.
- `JsonApiWriter` transforms resource objects into json response.

# Example
To get a sense of what this all looks like, let's look at an example query string:

```
/api/v1/blogs?
  include=owner,posts.comments.author&
  filter=has(posts)&
  sort=count(posts)&
  page[number]=3&
  fields[blogs]=title&
    filter[posts]=and(not(equals(author.userName,null)),has(comments))&
    sort[posts]=author.displayName&
    fields[blogPosts]=url&
      filter[posts.comments]=and(greaterThan(createdAt,'2001-01-01Z'),startsWith(author.userName,'J'))&
      sort[posts.comments]=-createdAt,author.displayName&
      fields[comments]=createdAt
```

After parsing, the set of scoped expressions is transformed into the following tree by `QueryLayerComposer`:

```
QueryLayer<Blog>
{
  Include: owner,posts.comments.author
  Filter: has(posts)
  Sort: count(posts)
  Pagination: Page number: 3, size: 5
  Selection
  {
    FieldSelectors<Blog>
    {
      title
      id
      posts: QueryLayer<BlogPost>
      {
        Filter: and(not(equals(author.userName,null)),has(comments))
        Sort: author.displayName
        Pagination: Page number: 1, size: 5
        Selection
        {
          FieldSelectors<BlogPost>
          {
            url
            id
            comments: QueryLayer<Comment>
            {
              Filter: and(greaterThan(createdAt,'2001-01-01'),startsWith(author.userName,'J'))
              Sort: -createdAt,author.displayName
              Pagination: Page number: 1, size: 5
              Selection
              {
                FieldSelectors<Comment>
                {
                  createdAt
                  id
                  author: QueryLayer<WebAccount>
                  {
                  }
                }
              }
            }
          }
        }
      }
      owner: QueryLayer<WebAccount>
      {
      }
    }
  }
}
```

Next, the repository translates this into a LINQ query that the following C# code would represent:

```c#
IQueryable<Blog> query = dbContext.Blogs
    .Include("Posts.Comments.Author")
    .Include("Owner")
    .Where(blog => blog.Posts.Any())
    .OrderBy(blog => blog.Posts.Count)
    .Skip(10)
    .Take(5)
    .Select(blog => new Blog
    {
        Title = blog.Title,
        Id = blog.Id,
        Posts = blog.Posts
            .Where(blogPost => blogPost.Author.UserName != null && blogPost.Comments.Any())
            .OrderBy(blogPost => blogPost.Author.DisplayName)
            .Take(5)
            .Select(blogPost => new BlogPost
            {
                Url = blogPost.Url,
                Id = blogPost.Id,
                Comments = blogPost.Comments
                    .Where(comment => comment.CreatedAt > DateTime.Parse("2001-01-01Z") &&
                        comment.Author.UserName.StartsWith("J"))
                    .OrderByDescending(comment => comment.CreatedAt)
                    .ThenBy(comment => comment.Author.DisplayName)
                    .Take(5)
                    .Select(comment => new Comment
                    {
                        CreatedAt = comment.CreatedAt,
                        Id = comment.Id,
                        Author = comment.Author
                    }).ToHashSet()
            }).ToList(),
        Owner = blog.Owner
    });
```

The LINQ query gets translated by Entity Framework Core into the following SQL:

```sql
SELECT t."Title", t."Id", a."Id", t2."Url", t2."Id", t2."Id0", t2."CreatedAt", t2."Id1", t2."Id00", t2."DateOfBirth", t2."DisplayName", t2."EmailAddress", t2."Password", t2."PersonId", t2."PreferencesId", t2."UserName", a."DateOfBirth", a."DisplayName", a."EmailAddress", a."Password", a."PersonId", a."PreferencesId", a."UserName"
FROM (
    SELECT b."Id", b."OwnerId", b."Title", (
        SELECT COUNT(*)::INT
        FROM "Posts" AS p0
        WHERE b."Id" = p0."ParentId") AS c
    FROM "Blogs" AS b
    WHERE EXISTS (
        SELECT 1
        FROM "Posts" AS p
        WHERE b."Id" = p."ParentId")
    ORDER BY (
        SELECT COUNT(*)::INT
        FROM "Posts" AS p0
        WHERE b."Id" = p0."ParentId")
    LIMIT @__Create_Item1_1 OFFSET @__Create_Item1_0
) AS t
LEFT JOIN "Accounts" AS a ON t."OwnerId" = a."Id"
LEFT JOIN LATERAL (
    SELECT t0."Url", t0."Id", t0."Id0", t1."CreatedAt", t1."Id" AS "Id1", t1."Id0" AS "Id00", t1."DateOfBirth", t1."DisplayName", t1."EmailAddress", t1."Password", t1."PersonId", t1."PreferencesId", t1."UserName", t0."DisplayName" AS "DisplayName0", t1."ParentId"
    FROM (
        SELECT p1."Url", p1."Id", a0."Id" AS "Id0", a0."DisplayName"
        FROM "Posts" AS p1
        LEFT JOIN "Accounts" AS a0 ON p1."AuthorId" = a0."Id"
        WHERE (t."Id" = p1."ParentId") AND (((a0."UserName" IS NOT NULL)) AND EXISTS (
            SELECT 1
            FROM "Comments" AS c
            WHERE p1."Id" = c."ParentId"))
        ORDER BY a0."DisplayName"
        LIMIT @__Create_Item1_1
    ) AS t0
    LEFT JOIN (
        SELECT t3."CreatedAt", t3."Id", t3."Id0", t3."DateOfBirth", t3."DisplayName", t3."EmailAddress", t3."Password", t3."PersonId", t3."PreferencesId", t3."UserName", t3."ParentId"
        FROM (
            SELECT c0."CreatedAt", c0."Id", a1."Id" AS "Id0", a1."DateOfBirth", a1."DisplayName", a1."EmailAddress", a1."Password", a1."PersonId", a1."PreferencesId", a1."UserName", c0."ParentId", ROW_NUMBER() OVER(PARTITION BY c0."ParentId" ORDER BY c0."CreatedAt" DESC, a1."DisplayName") AS row
            FROM "Comments" AS c0
            LEFT JOIN "Accounts" AS a1 ON c0."AuthorId" = a1."Id"
            WHERE (c0."CreatedAt" > @__Create_Item1_2) AND ((@__Create_Item1_3 = '') OR (((a1."UserName" IS NOT NULL)) AND ((a1."UserName" LIKE @__Create_Item1_3 || '%' ESCAPE '') AND (left(a1."UserName", length(@__Create_Item1_3))::text = @__Create_Item1_3::text))))
        ) AS t3
        WHERE t3.row <= @__Create_Item1_1
    ) AS t1 ON t0."Id" = t1."ParentId"
) AS t2 ON TRUE
ORDER BY t.c, t."Id", a."Id", t2."DisplayName0", t2."Id", t2."Id0", t2."ParentId", t2."CreatedAt" DESC, t2."DisplayName", t2."Id1"
```
