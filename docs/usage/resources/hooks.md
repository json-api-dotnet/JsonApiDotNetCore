A first version of the usage guide can be found below (or [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/876529b92a46932ee549fd474e53785ca73d8ada/docs/usage/resources/hooks.md)). This should help when reviewing this PR.

# Resource Hooks
The ResourceDefinition class now supports hooks that can be executed just before and right after common CRUD operations. They allow for a  convenient declaration of business logic without having to override the service layer. Resource hooks were introduced in v3.2.0.

This section covers the usage of these hooks. Note that the hooks are a part of `ResourceDefinition`. See the [ResourceDefinition usage guide](...) for details on how to setup a `ResourceDefiniton` in general.

## Available hooks
For every CRUD operation on a resource, a `Before` and `After` hook is available:
* [BeforeCreate](https://www.link-to-generated-api-spec.com)
* [AfterCreate](https://www.link-to-generated-api-spec.com)
* [BeforeRead](https://www.link-to-generated-api-spec.com)
* [AfterRead](https://www.link-to-generated-api-spec.com)
* [BeforeUpdate](https://www.link-to-generated-api-spec.com)
* [AfterUpdate](https://www.link-to-generated-api-spec.com)
* [BeforeDelete](https://www.link-to-generated-api-spec.com)
* [AfterDelete](https://www.link-to-generated-api-spec.com)

These hooks, if defined, are executed by the `EntityResourceService`. 

Sometimes multiple hooks are involved for a single CRUD operation. For example, if we`create` a new `Article` and simultaneously set a relationship to an existing `Author`,  then hooks for both `Article`  and `Author` are fired:  
* the `BeforeCreate` and `AfterCreate`  hooks for `Article` 
* the `BeforeUpdate ` and `AfterUpdate` hooks for `Author`

For more information about involved hooks per CRUD operation, see below.

# TODO

* Tidy up the structure for easy learning
* Edge cases should be explained
* what CANT I do?
* Explanation of being forward looking

## Hooks involved per CRUD action
The hooks are executed by the `EntityResourceService` are different per method. Here is an overview of which hooks are involved in which scenario.

### CRUD operations on resource
#### EntityResourceService.CreateAsync(TEntity entity)
* endpoint: `CREATE /articles`
* hooks involved:
    *  [BeforeCreate](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks (when simultaneously creating relationships to existing entities): [BeforeUpdate](https://www.link-to-generated-api-spec.com)
    *  [AfterCreate](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks (when simultaneously creating relationships to existing entities): [AfterUpdate](https://www.link-to-generated-api-spec.com)

#### EntityResourceService.GetAsync()` (and `.GetAsync(TId id)`
* endpoint: `GET /articles`  (and `GET /articles/{id}` )
* hooks involved:
    *  [BeforeRead](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks: N/A (no data has been retrieved yet)
    *  [AfterRead](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks (when doing `?include=` for arbitrarily deep relationship chain): [BeforeRead](https://www.link-to-generated-api-spec.com), [AfterRead](https://www.link-to-generated-api-spec.com)



#### EntityResourceService.UpdateAsync(TEntity entity)
* endpoint: `PATCH /articles/{id}`
* hooks involved:
    *  [BeforeUpdate](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks: (when simultaneously updating relationships to existing entities) [BeforeUpdate](https://www.link-to-generated-api-spec.com)
    *  [AfterUpdate](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks: (when simultaneously updating relationships to existing entities) [AfterUpdate](https://www.link-to-generated-api-spec.com)

#### EntityResourceService.DeleteAsync(TId id)
* endpoint: `DELETE /articles/{id}`
* hooks involved:
    *  [BeforeDelete](https://www.link-to-generated-api-spec.com) for the root entity
        * no nested manipulation supported in JSON:API spec
    *  [AfterCreate](https://www.link-to-generated-api-spec.com) for the root entity
        * no nested manipulation supported in JSON:API spec

### CRUD operations on relationships

#### GetRelationshipAsync
* endpoint: `GET /articles/1/{relationshipName}`
* hooks involved:
    *  [BeforeRead](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks: N/A (no data has been retrieved yet)
    *  [AfterRead](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks (for related entities): [BeforeRead](https://www.link-to-generated-api-spec.com), [AfterRead](https://www.link-to-generated-api-spec.com)


#### UpdateRelationshipsAsync
* endpoint: `GET /articles/1/{relationshipName}`
* hooks involved:
    *  [BeforeUpdate](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks (for related entities): [BeforeUpdate](https://www.link-to-generated-api-spec.com)
    *  [AfterUpdate](https://www.link-to-generated-api-spec.com) for the root entity
        * nested hooks (for related entities): [AfterUpdate](https://www.link-to-generated-api-spec.com)



## Simple usage: `BeforeDelete` and `AfterDelete`

The following example shows a simple use case of the `BeforeDelete` and `AfterDelete` hooks. 
* In the `BeforeDelete` hook we perform an authorization check
* In both the `BeforeDelete` and `AfterDelete` hooks we perform custom logging. 

These hooks are implemented on `ArticleResource` by overriding the corresponding virtual implementations on `ResourceDefinition<TEntity>`. Note that if the virtual implementations are not overriden,  the `EntityResourceService` will not execute them.

```c#
public class ArticleResource : ResourceDefinition<Article>
{
    readonly ILogger _logger;
    public ArticleResource(IHooksDiscovery<Article> hooks = null,
        ILogger logger) : base(hooks) 
    {
        _logger = logger
     }

    public override void BeforeDelete(IEnumerable<Article> entities, ResourceAction actionSource)
    {
        if (entities.Single().Name == "Classified")
        {
            _logger.info("Someone tried to delete a classified article!")
            throw new JsonApiException(401, "Not Allowed", new UnauthorizedAccessException());
        }
    }

    public override void AfterDelete(IEnumerable<Article> entities, bool succeeded, ResourceAction actionSource)
    {
        string name = entities.Single().Name;
        if (succeeded) {
            _logger.info("Article with name {name} was deleted")
        } else {
            _logger.info("Deletion of article with name {name} failed")
        }
    }
}
```

There are a few important things to note.

**Dependency injection**
The lifetime cycle of `ResourceDefinition` is `scoped`, therefore you access any service through dependency injection

**IHooksDiscovery**
When using resource hooks in `ResourceDefinition`, you **MUST** inject and pass along `IHooksDiscovery<TEntity>` to the base constructor.

**Parameter count**
Almost all hooks accept a parameter of type `IEnumerable<TEnity>`  (and some of them also have a return value of that same type).

Even though an `IEnumerable` can be of arbitrary length of in the case of `BeforeDelete` and `AfterDelete` it will **at most** contain one element. This is because this hook is related to the `DELETE article/{id}` endpoint, which allows for deletion of only one resource per request. Although at compile time it is possible to return more than one entity from this method, _this is not allowed and at runtime this will generate an error_.

The reason why we are using parameter `IEnumerable<TEntity>` and not `TEntity` is because in more complex usage scenarios, it is possible for the hooks to get executed with more than one entity, as will be shown in nexts example. Therefore, for consistency and to not confuse the developer with a large amount of overloads for different scenarios all hooks work with `IEnumerable<TEntity>`.


## Complex usage 1: performing a filtered include

Entity Framework Core does currently not support [filtered includes](https://github.com/aspnet/EntityFrameworkCore/issues/1833). The same end result can be achieved with the `AfterRead` hook. 

Consider a `GET` request on `/article?include=tags` endpoint where the relation between `Article` and `Tag` is `many-to-many` . In our example, we will adjust the api response by censoring all all `Article`s that have "classified" as title, and all `Tag`s that have label "secret" 

```c#
public class ArticleResource : ResourceDefinition<Article>
{
   public ArticleResource(IHooksDiscovery<Article> hooks = null) : base(hooks) { }

   public override void BeforeRead(ResourceAction actionSource, string stringId = null)
   {
       Console.WriteLine("Article BeforeRead executed!")
   }

   public override IEnumerable<Article> AfterRead(IEnumerable<Article> entities, ResourceAction actionSource)
   {
       Console.WriteLine("Article AfterRead executed!")
       return entities.Where(a => a.Name != "classified");
   }
}

public class TagResource : ResourceDefinition<Tag>
{
   public override void BeforeRead(ResourceAction actionSource, string stringId = null)
   {
       Console.WriteLine("Tag BeforeRead executed!")
   }

   public override IEnumerable<Tag> AfterRead(IEnumerable<Tag> entities, ResourceAction actionSource)
   {
       Console.WriteLine("Tag AfterRead executed!")
       return entities.Where(a => a.Label != TagLabel.Secret);
   }
}
```
To illustrate how these hooks will operate, consider a dataset of 3 articles at the "root level" with 6 unique tags at the "child" (8 in total because tags with id 2 and 3 occur twice), as schematically represented by the following JSON object:

```json
[  
   {  
      "id":1,
      "articleName":"classified",
      "tags":[  
         {  
            "id":1,
            "label":"secret"
         },
         {  
            "id":2,
            "label":"public"
         },
         {  
            "id":3,
            "label":"public"
         }
      ]
   },
   {  
      "id":2,
      "articleName":"some name",
      "tags":[  
         {  
            "id":4,
            "label":"secret"
         },
         {  
            "id":5,
            "label":"public"
         },
         {  
            "id":6,
            "label":"public"
         }
      ]
   },
   {  
      "id":3,
      "articleName":"another name",
      "tags":[  
         {  
            "id":2,
            "label":"public"
         },
         {  
            "id":5,
            "label":"public"
         }
      ]
   }
]
```

The hooks in the above example will result in the following:
* For the root level of the dataset, the `Article` `AfterRead` hook will 
    * fire **exactly once**: the parameter `IEnumerable<Article> entities`will contain all three articles;
    * remove article with id 1 from the dataset. 
* For the children level in this dataset, the `Tag` `AfterRead` hook will contain 
    * fire **exactly once** : the parameter `IEnumerable<Tag> entities` will contain tags with id 2, 4, 5 and 6. 
        * Note that ids 1 and 3 are excluded because these belonged to the classified article.
        * Tag with id 2 is still included because it was also included with the last article.
        * Tag with id 5 will not occur twice: only unique entities will be provided to the hook.
    * it will remove tag with id 4 from the dataset

The following order of `Console.WriteLine( ... )` statements would be observed:

```bash
Article BeforeRead executed!
Article AfterRead executed!
Tag BeforeRead executed!
Tag AfterRead executed!
```

following dataset would be the result:

```json
[  
   {  
      "id":"2",
      "articleName":"some name",
      "tags":[  
         {  
            "id":5,
            "label":"public"
         },
         {  
            "id":6,
            "label":"public"
         }
      ]
   },
   {  
      "id":"3",
      "articleName":"another name",
      "tags":[  
         {  
            "id":2,
            "label":"public"
         },
         {  
            "id":5,
            "label":"public"
         }
      ]
   }
]
```

## Complex usage 2: leveraging action source
Some hooks are shared between multiple CRUD operations. To illustrate this consider the case of updating `Author`s. 

When updating for example the name of an `Author`  through the `PATCH authors/{id}` endpoint, the `BeforeUpdate` and `AfterUpdate` hooks for `Author ` will be executed. However, when creating a new `Article` and relating it to an existing `Author`  through the `POST articles/{id}` endpoint,  the `BeforeUpdate` and `AfterUpdate` for `Author` will also be executed.

It is possible to distinguish between these scenarios by using the `actionSource` parameter. 

```c#
public class AuthorResource : ResourceDefinition<Author>
{
    public ArticleResource(IHooksDiscovery<Author> hooks = null) : base(hooks) { }

    public override void BeforeUpdate(IEnumerable<Author> entities, ResourceAction actionSource)
    {
        if (actionSource == ResourceAction.Post)
        {
            Console.WriteLine("An entity was created and a relationship to      author was made");
        } else if (actionSource == ResourceAction.Patch) 
        {
            Console.WriteLine("An author was updated directly");
        }
    }
}
```


## Complex usage 3: Hooks on jointables
In the case of `many-to-many` it is possible to define hooks for the `Through` type, eg `ArticleTags`  (see example Complex usage 1). This might be useful when one has metadata on the jointable. In this case, the normal `Through` type `ArticleTag` as below,

```c#
    public class ArticleTag
    {
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
```
must be changed to the version below that inherits from `Identifiable` and uses `RelationshipAttribute`.
```c#
 public class IdentifiableArticleTag : Identifiable
 {
     public int ArticleId { get; set; }
     [HasOne("article")]
     public Article Article { get; set; }
     public int TagId { get; set; }
     [HasOne("Tag")]
     public Tag Tag { get; set; }
     public string SomeMetaData { get; set; }
 }
```

Then hooks can be defined as follows:
```c#
public class ArticleTagResource : ResourceDefinition<IdentifiableArticleTag>
{
    public override void BeforeRead(ResourceAction actionSource, string stringId = null)
    {
        Console.WriteLine("IdentifiableArticleTag BeforeRead executed!")
    }

    public override IEnumerable<IdentifiableArticleTag> AfterRead(IEnumerable<IdentifiableArticleTag> entities, ResourceAction actionSource)
    {
        Console.WriteLine("IdentifiableArticleTag AfterRead executed!")
        return entities
    }
}
```
And the call `Console.WriteLine()` statements as in example Complex usage 1 will look like this
```bash
Article BeforeRead executed!
Article AfterRead executed!
IdentifiableArticleTag BeforeRead executed!
IdentifiableArticleTag AfterRead executed!
Tag BeforeRead executed!
Tag AfterRead executed!
```
The resulting dataset, however, will in this case look exactly the same.