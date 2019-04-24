# Resource Hooks

The ResourceDefinition class now supports hooks that can be executed right before and right after common operations. They allow to conveniently define business logic without having to override the service layer. It was been introduced in v3.2.0.

This section covers the usage of these hooks, which is a part of `ResourceDefinition`. See the [ResourceDefinition usage guide](...) for details on how to use `ResourceDefiniton` in general.

## Available hooks
For every CRUD operation on a resource, a `Before` and `After` hook is available:
* [BeforeCreate]( link to generated api spec )
* [AfterCreate]( link to generated api spec )
* [BeforeRead]( link to generated api spec )
* [AfterRead]( link to generated api spec )
* [BeforeUpdate]( link to generated api spec )
* [AfterUpdate]( link to generated api spec )
* [BeforeDelete]( link to generated api spec )
* [AfterDelete]( link to generated api spec )
These hooks, if defined, are executed by the `EntityResourceService`.

## Simple usage

The following example shows a simple usecase of the before and after deletion hooks. In the `BeforeDelete` hook authorization is peformed, and in both the `BeforeDelete` and `AfterDelete` some custom logging is performed. These hooks are implemented on `ArticleResource` by overriding the virtual implementation on `ResourceDefinition<TEntity>` (which is ignored by the `EntityResourceService`: only when a hooks is overriden, it is executed in the service layer).

```c#
    public class ArticleResource : ResourceDefinition<Article>
    {
        readonly ILoggerService _logger;
        public ArticleResource(IHooksDiscovery<Article> hooks = null,
            ILoggerService logger) : base(hooks) 
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

**IHooksDiscovery**
When using resource hooks, you **must** inject and pass along `IHooksDiscovery<TEntity>` to the base constructor.

**Parameter count**
Here, in the case of before and after deletion hooks, the parameter `IEnumerable<Article> entities` will always **at most** one element. This is because this hook gets executed when the `DELETE` verb is used, and as per the JSON:API we can only delete one resource per request. Although at compile time it is possible to return more than one entity from this method, at runtime this will generate an error.

The reason why we are using parameter `IEnumerable<TEntity>` and not `TEntity` is because in more complex usage scenarios, it is possible for the hooks to get executed with more than one entity, as will be shown in the next example. Therefore, for consistency and to not confuse the developer with a large amount of overloads that get executed in different scenarios, all hooks work with `IEnumerable<TEntity>`.


## Complex usage 1: performing a filtered include

Entity Framework Core does currently not support [filtered includes](https://github.com/aspnet/EntityFrameworkCore/issues/1833). The same end result can be achieved with the `AfterRead` hook. 

Consider a `GET` request on `/article?include=tags` endpoint where the relation between `Article` and `Tag` is `many-to-many` (hence there is a `HasManyThrough` attribute associate with `ArticleTag). In our example, we will adjust the api response by censoring all all `Article`s that have "Classified" as title, and all `Tag`s that have label "Secret" 

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
        public TagResource(IHooksDiscovery<Tag> hooks = null) : base(hooks) { }
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

Consider a dataset of 3 articles with 6 unique tags, as schematically represented by the following json object:

```json
[
   {
      "id":"1",
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
      "id":"2",
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

As a result of the resource hooks as defined in the above example, the following will happen
* For the root level of the dataset, the `Article` `AfterRead` hook will fire once and will censor the article with id = 1. 
* Note that parameter `IEnumerable<Article> entities` in this hook wil contain all three articles.
* For the children level in this dataset, the `Tag` `AfterRead` hook will remove the tag with id = 4 from article with id = 2. 
* Note that parameter `IEnumerable<Tag> entities` in this hook wil contain all 6 tags. Note also that tag with id = 5 and id = 2 will not occur twice in this collection: a hook is executed once for all unique entities within a layer of the targeted dataset.

The following order of `Console.WriteLine( ... )` statements would be observed:
```bash
Article BeforeRead executed!
Tag BeforeRead executed!
Tag AfterRead executed!
Article AfterRead executed!
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

## Complex usage 2: Many-To-Many with hooks join table hooks
It is possible to define hooks for the `Through` type, eg `ArticleTags` in the above example. This might be useful when one has metadata on the jointable. In this case, the normal `Through` type `ArticleTag` as below,

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
        public ArticleResource(IHooksDiscovery<IdentifiableArticleTag> hooks = null) : base(hooks) { }

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
And the call `Console.WriteLine()` statements as in previous example will look like this
```bash
Article BeforeRead executed!
IdentifiableArticleTag BeforeRead executed!
IdentifiableArticleTag AfterRead executed!
Tag BeforeRead executed!
Tag AfterRead executed!
Article AfterRead executed!
```
The resulting dataset, however, will in this case look exactly the same.




