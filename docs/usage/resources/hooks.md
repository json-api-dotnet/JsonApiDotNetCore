

# Resource Hooks
This section covers the usage of **Resource Hooks**, which is a feature of`ResourceHooksDefinition<T>`. See the [ResourceDefinition usage guide](resource-definitions.md) for a general explanation on how to set up a `JsonApiResourceDefinition<T>`. For a quick start, jump right to the [Getting started: most minimal example](#getting-started-most-minimal-example) section.

> Note: Resource Hooks are an experimental feature and are turned off by default. They are subject to change or be replaced in a future version.

By implementing resource hooks on a `ResourceHooksDefintion<T>`, it is possible to intercept the execution of the **Resource Service Layer** (RSL) in various ways. This enables the developer to conveniently define business logic without having to override the RSL. It can be used to implement e.g.
* Authorization
* [Event-based synchronisation between microservices](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/integration-event-based-microservice-communications)
* Logging
* Transformation of the served data

This usage guide covers the following sections
1.  [**Semantics: pipelines, actions and hooks**](#1-semantics-pipelines-actions-and-hooks)
Understanding the semantics will be helpful in identifying which hooks on `ResourceHooksDefinition<T>` you need to implement for your use-case.
2.  [**Basic usage**](#2-basic-usage)
        Some examples to get you started.
      * [**Getting started: most minimal example**](#getting-started-most-minimal-example)
      * [**Logging**](#logging)
      * [**Transforming data with OnReturn**](#transforming-data-with-onreturn)
      * [**Loading database values**](#loading-database-values)
3.  [**Advanced usage**](#3-advanced-usage)
    Complicated examples that show the advanced features of hooks.
      * [**Simple authorization: explicitly affected resources**](#simple-authorization-explicitly-affected-resources)
      * [**Advanced authorization: implicitly affected resources**](#advanced-authorization-implicitly-affected-resources)
      * [**Synchronizing data across microservices**](#synchronizing-data-across-microservices)
      * [**Hooks for many-to-many join tables**](#hooks-for-many-to-many-join-tables)
5.  [**Hook execution overview**](#4-hook-execution-overview)
  A table overview of all pipelines and involved hooks

# 1. Semantics: pipelines, actions and hooks

## Pipelines
The different execution flows within the RSL that may be intercepted can be identified as **pipelines**. Examples of such pipelines are
* **Post**: creation of a resource (triggered by the endpoint `POST /my-resource`).
* **PostBulk**: creation of multiple resources (triggered by the endpoint `POST /bulk/my-resource`).
  * *NB: hooks are not yet supported with bulk operations.*
* **Get**: reading a resource (triggered by the endpoint `GET /my-resource`).
* **GetSingle**: reading a single resource (triggered by the endpoint `GET /my-resource/1`).

See the [ResourcePipeline](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/745d2fb6b6c9dd21ff794284a193977fdc699fe6/src/JsonApiDotNetCore/Hooks/Internal/Execution/ResourcePipeline.cs) enum for a full list of available pipelines.

## Actions
Each pipeline is associated with a set of **actions** that work on resources and their relationships. These actions reflect the associated database operations that are performed by JsonApiDotNetCore (in the Repository Layer). Typically, the RSL will execute some service-layer-related code, then invoke the Repository Layer which will perform these actions, after which the execution returns to the RSL.

Note that some actions are shared across different pipelines, and note that most pipelines perform multiple actions.  There are two types of actions: **primary resource actions** and **nested resource actions**.

### Primary resource actions
Most actions are trivial in the context of the pipeline where they're executed from. They may be recognised as the familiar *CRUD* operations of an API. These actions are:

* The `create` action: the **Post** pipeline will `create` a resource
* The `read` action: the **Get** and  **GetSingle** pipeline will `read` (a) resource(s).
* The `update` action: the **Patch** pipeline will `update` a resource.
* The `delete` action: the **Delete** pipeline will `delete` a resource.

These actions are called the **primary resource actions** of a particular pipeline because **they act on the request resource**. For example, when an `Article` is created through the **Post** pipeline, its main action, `create`, will work on that `Article`.

### Nested Resource Actions
Some other actions might be overlooked, namely the nested resource actions. These actions are

*  `update relationship`  for directly affected relationships
*  `implicit update relationship` for implicitly affected relationships
* `read` for included relationships

These actions are called **nested resource actions** of a particular pipeline because **they act on involved (nested) resources** instead of the primary request resource. For example, when loading articles and their respective authors (`GET /articles?include=author`), the `read` action on `Article` is the primary action, and the `read` action on `Person` is the nested action.

#### The `update relationship` action
[As per the Json:Api specification](https://jsonapi.org/format/#crud-creating](https://jsonapi.org/format/#crud-creating), the **Post** pipeline also allows for an `update relationship` action on an already existing resource. For example, when creating an `Article` it is possible to simultaneously relate it to an existing `Person` by setting its author. In this case, the `update relationship` action is a nested action that will work on that `Person`.

#### The `implicit update relationship` action
the **Delete**  pipeline also allows for an `implicit update relationship` action on an already existing resource. For example, for an  `Article` that its author property assigned to a particular `Person`,  the relationship between them is destroyed when this article is deleted. This update is "implicit" in the sense that no explicit information about that `Person` was provided in the request that triggered this pipeline. An `implicit update relationship` action is therefore performed on that `Person`.  See [this section](#advanced-authorization-implicitly-affected-resources) for a more detailed explanation.

### Shared actions
Note that **some actions are shared across pipelines**. For example, both the **Post** and **Patch** pipeline can perform the `update relationship`  action on an (already existing) involved resource. Similarly, the **Get** and **GetSingle** pipelines perform the same `read` action.
<br><br>
For a complete list of actions associated with each pipeline, see the [overview table](#4-hook-execution-overview).

## Hooks
For all actions it is possible to implement **at least one hook** to intercept its execution. These hooks can be implemented by overriding the corresponding virtual  implementation on `ResourceHooksDefintion<T>`. (Note that the base implementation is a dummy implementation, which is ignored when firing hooks.)

### Action related hooks
As an example, consider the `create` action for the `Article` Resource. This action can be intercepted by overriding the
* `ResourceHooksDefinition<Article>.BeforeCreate` hook for custom logic **just before** execution of the main `create` action
* `ResourceHooksDefinition<Article>.AfterCreate` hook for custom logic **just after** execution of the main `create` action

If with the creation of an `Article`  a relationship to `Person` is updated simultaneously, this can be intercepted by overriding the
* `ResourceHooksDefinition<Person>.BeforeUpdateRelationship` hook for custom logic **just before** the execution of the nested `update relationship` action.
* `ResourceHooksDefinition<Person>.AfterUpdateRelationship` hook for custom logic **just after** the execution of the nested `update relationship` action.

### OnReturn hook
As mentioned in the previous section, some actions are shared across hooks. One of these actions is the `return` action. Although not strictly compatible with the *CRUD* vocabulary, and although not executed by the Repository Layer, pipelines are also said to perform a `return` action when any content is to be returned from the API. For example, the **Delete** pipeline does not return any content, but a *HTTP 204 No Content* instead, and will therefore not perform a `return` action. On the contrary, the **Get** pipeline does return content, and will therefore perform a `return action`

Any return content can be intercepted and transformed as desired by implementing the `ResourceHooksDefinition<TEntity>.OnReturn` hook which intercepts the `return` action. For this action, there is no distinction between a `Before` and `After` hook, because no code after a `return` statement can be evaluated. Note that the `return` action can work on *primary resources as well as nested resources*, see  [this example below](#transforming-data-with-onreturn).
<br><br>
For an overview of all pipelines, hooks and actions, see the table below, and for more detailed information about the available hooks, see the [IResourceHookContainer<T>](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/ab1f96d8255532461da47d290c5440b9e7e6a4a5/src/JsonApiDotNetCore/Hooks/IResourceHookContainer.cs) interface.

# 2. Basic usage

## Getting started: most minimal example
To use resource hooks, you are required to turn them on in your `startup.cs` configuration

```c#
public void ConfigureServices(IServiceCollection services)
{
    // ...

    services.AddJsonApi<ApiDbContext>(options =>
    {
        options.EnableResourceHooks = true; // default is false
        options.LoadDatabaseValues = false; // default is false
    });

    // ...
}
```

For this example, we may set `LoadDatabaseValues` to `false`. See the [Loading database values](#loading-database-values) example for more information about this option.

The simplest case of resource hooks we can then implement should not require a lot of explanation. This hook would be triggered by any default JsonApiDotNetCore API route for `Article`.

```c#
public class ArticleResource : ResourceHooksDefinition<Article>
{
    public override IEnumerable<Article> OnReturn(HashSet<Article> entities,
        ResourcePipeline pipeline)
    {
        Console.WriteLine("This hook does not do much apart from writing this message" +
             " to the console just before serving the content.");
        return entities;
    }
}
```

## Logging
This example shows how some actions can be logged on the level of API users.

First consider the following scoped service which creates a logger bound to a particular user and request.

```c#
/// This is a scoped service, which means log will have a request-based
/// unique id associated to it.
public class UserActionsLogger : IUserActionsLogger
{
    public ILogger Instance { get; private set; }

    public UserActionsLogger(ILoggerFactory loggerFactory, IUserService userService)
    {
        var userId = userService.GetUser().Id;
        Instance =
            loggerFactory.CreateLogger($"[request: {Guid.NewGuid()}" + "user: {userId}]");
    }
}
```

Now, let's assume our application has two resources: `Article` and `Person`, and that there exist a one-to-one  and one-to-many relationship between them (`Article` has one `Author` and `Article` has many `Reviewers`). Let's assume we are required to log the following events:
* An API user deletes an article
* An API user removes the `Author` relationship of a person
* An API user removes the `Reviewer` relationship of a person

This could be achieved in the following way:

```c#
/// Note that resource definitions are also registered as scoped services.
public class ArticleResource : ResourceHooksDefinition<Article>
{
    private readonly ILogger _userLogger;

    public ArticleResource(IUserActionsLogger logService)
    {
        _userLogger = logService.Instance;
    }

    public override void AfterDelete(HashSet<Article> entities, ResourcePipeline pipeline,
        bool succeeded)
    {
        if (!succeeded)
        {
            return;
        }

        foreach (Article article in entities)
        {
            _userLogger.Log(LogLevel.Information,
                $"Deleted article '{article.Name}' with id {article.Id}");
        }
    }
}

public class PersonResource : ResourceHooksDefinition<Person>
{
    private readonly ILogger _userLogger;

    public PersonResource(IUserActionsLogger logService)
    {
        _userLogger = logService.Instance;
    }

    public override void AfterUpdateRelationship(
        IAffectedRelationships<Person> resourcesByRelationship, ResourcePipeline pipeline)
    {
        var updatedRelationshipsToArticle = relationshipHelper.EntitiesRelatedTo<Article>();

        foreach (var updated in updatedRelationshipsToArticle)
        {
            RelationshipAttribute relationship = updated.Key;
            HashSet<Person> affectedEntities = updated.Value;

            foreach (Person person in affectedEntities)
            {
                if (pipeline == ResourcePipeline.Delete)
                {
                    _userLogger.Log(LogLevel.Information,
                        $"Deleted the {relationship.PublicRelationshipName} relationship " +
                        $"to Article for person '{person.FirstName} {person.LastName}' " +
                        $"with id {person.Id}");
                }
            }
        }
    }
}
```

If eg. an API user deletes an article with title *JSON:API paints my bikeshed!* that had related as author *John Doe* and as reviewer *Frank Miller*, the logs generated logs would look something like

```
[request: 186190e3-1900-4329-9181-42082258e7b4, user: dd1cd99d-60e9-45ca-8d03-a0330b07bdec] Deleted article 'JSON:API paints my bikeshed!' with id fac0436b-7aa5-488e-9de7-dbe00ff8f04d
[request: 186190e3-1900-4329-9181-42082258e7b4, user: dd1cd99d-60e9-45ca-8d03-a0330b07bdec] Deleted the author relationship to Article for person 'John Doe' with id 2ec3990d-c816-4d6d-8531-7da4a030d4d0
[request: 186190e3-1900-4329-9181-42082258e7b4, user: dd1cd99d-60e9-45ca-8d03-a0330b07bdec] Deleted the reviewer relationship to Article for person 'Frank Miller' with id 42ad6eb2-b813-4261-8fc1-0db1233e665f
```

## Transforming data with OnReturn
Using the `OnReturn` hook, any set of resources can be manipulated as desired before serving it from the API. One of the use-cases for this is being able to perform a [filtered include](https://github.com/aspnet/EntityFrameworkCore/issues/1833), which is currently not supported by Entity Framework Core.

As an example, consider again an application with the `Article`  and `Person` resource, and let's assume the following business rules:
* when reading `Article`s, we never want to show articles for which the `IsSoftDeleted` property is set to true.
* when reading `Person`s, we never want to show people who wish to remain anonymous (`IsAnonymous` is set to true).

This can be achieved as follows:

```c#
public class ArticleResource : ResourceHooksDefinition<Article>
{
    public override IEnumerable<Article> OnReturn(HashSet<Article> entities,
        ResourcePipeline pipeline)
    {
        return entities.Where(article => !article.IsSoftDeleted);
    }
}

public class PersonResource : ResourceHooksDefinition<Person>
{
    public override IEnumerable<Person> OnReturn(HashSet<Person> entities,
        ResourcePipeline pipeline)
    {
        if (pipeline == ResourcePipeline.Get)
        {
            return entities.Where(person => !person.IsAnonymous);
        }
        return entities;
    }
}
```

Note that not only anonymous people will be excluded when directly performing a `GET /people`, but also when included through relationships, like `GET /articles?include=author,reviewers`. Simultaneously, `if` condition that checks for `ResourcePipeline.Get` in the `PersonResource` ensures we still get expected responses from the API when eg. creating a person with `WantsPrivacy` set to true.

## Loading database values
When a hook is executed for a particular resource, JsonApiDotNetCore can load the corresponding database values and provide them in the hooks. This can be useful for eg.
 * having a diff between a previous and new state of a resource (for example when updating a resource)
 * performing authorization rules based on the property of a resource.

For example, consider a scenario with the following two requirements:
* We need to log all updates on resources revealing their old and new value.
* We need to check if the property `IsLocked` is set is `true`, and if so, cancel the operation.

Consider an `Article` with title *Hello there* and API user trying to update the the title of this article to *Bye bye*.  The above requirements could be implemented as follows:

```c#
public class ArticleResource : ResourceHooksDefinition<Article>
{
    private readonly ILogger _logger;
    private readonly ITargetedFields _targetedFields;

    public constructor ArticleResource(ILogger logger, ITargetedFields targetedFields)
    {
        _logger = logger;
        _targetedFields = targetedFields;
    }

    public override IEnumerable<Article> BeforeUpdate(IResourceDiff<Article> entityDiff,
        ResourcePipeline pipeline)
    {
        // PropertyGetter is a helper class that takes care of accessing the values
        // on an instance of Article using reflection.
        var getter = new PropertyGetter<Article>();

        // ResourceDiff<T> is like a list that contains ResourceDiffPair<T> elements
        foreach (ResourceDiffPair<Article> affected in entityDiff)
        {
            // the current state in the database
            var currentDatabaseState = affected.DatabaseValue;

            // the value from the request
            var proposedValueFromRequest = affected.Entity;

            if (currentDatabaseState.IsLocked)
            {
                throw new JsonApiException(403, "Forbidden: this article is locked!")
            }

            foreach (var attr in _targetedFields.Attributes)
            {
                var oldValue = getter(currentDatabaseState, attr);
                var newValue = getter(proposedValueFromRequest, attr);

                _logger.LogAttributeUpdate(oldValue, newValue)
            }
        }

        // You must return IEnumerable<Article> from this hook.
        // This means that you could reduce the set of entities that is
        // affected by this request, eg. by entityDiff.Entities.Where( ... );
        entityDiff.Entities;
    }
}
```

In this case the `ResourceDiffPair<T>.DatabaseValue` is `null`.  If you try to access all database values at once (`ResourceDiff.DatabaseValues`) when it is turned off, an exception will be thrown.

Note that database values are turned off by default. They can be turned on globally by configuring the startup as follows:

```c#
public void ConfigureServices(IServiceCollection services)
{
    // ...

    services.AddJsonApi<ApiDbContext>(options =>
    {
        options.LoadDatabaseValues = true;
    });

    // ...
}
```

The global setting can be used together with per-hook configuration hooks using the `LoadDatabaseValues` attribute:

```c#
public class ArticleResource : ResourceHooksDefinition<Article>
{
    [LoadDatabaseValues(true)]
    public override IEnumerable<Article> BeforeUpdate(IResourceDiff<Article> entityDiff,
        ResourcePipeline pipeline)
    {
        // ...
    }

    [LoadDatabaseValues(false)]
    public override IEnumerable<string> BeforeUpdateRelationships(HashSet<string> ids,
        IAffectedRelationships<Article> resourcesByRelationship, ResourcePipeline pipeline)
    {
        // the entities stored in the IAffectedRelationships<Article> instance
        // are plain resource identifier objects when LoadDatabaseValues is turned off,
        // or objects loaded from the database when LoadDatabaseValues is turned on.
     }
  }
}
```

Note that there are some hooks that the `LoadDatabaseValues` option and attribute does not affect. The only hooks that are affected are:
* `BeforeUpdate`
* `BeforeUpdateRelationship`
* `BeforeDelete`


# 3. Advanced usage

## Simple authorization: explicitly affected resources
Resource hooks can be used to easily implement authorization in your application.  As an example, consider the case in which an API user is not allowed to see anonymous people, which is reflected by the `Anonymous` property on `Person` being set to `true`. The API should handle this as follows:
* When reading people (`GET /people`), it should hide all people that are set to anonymous.
* When reading a single person (`GET /people/{id}`), it should throw an authorization error if the particular requested person is set to anonymous.

This can be achieved as follows:

```c#
public class PersonResource : ResourceHooksDefinition<Person>
{
    private readonly _IAuthorizationHelper _auth;

    public constructor PersonResource(IAuthorizationHelper auth)
    {
      // IAuthorizationHelper is a helper service that handles all authorization related logic.
      _auth = auth;
    }

    public override IEnumerable<Person> OnReturn(HashSet<Person> entities,
        ResourcePipeline pipeline)
    {
        if (!_auth.CanSeeSecretPeople())
        {
            if (pipeline == ResourcePipeline.GetSingle)
            {
                throw new JsonApiException(403, "Forbidden to view this person",
                    new UnauthorizedAccessException());
            }

            entities = entities.Where(person => !person.IsSecret)
        }

        return entities;
    }
}
```

This example of authorization is considered simple because it only involves one resource. The next example shows a more complex case.

## Advanced authorization: implicitly affected resources
Let's consider an authorization scenario for which we are required to implement multiple hooks across multiple resource definitions. We will assume the following:
* There exists a one-to-one relationship between `Article` and `Person`: an article can have only one author, and a person can be author of only one article.
* The author of article `Old Article` is person `Alice`.
* The author of article `New Article` is person `Bob`.

Now let's consider an API user that tries to update `New Article` by setting its author to `Alice`. The request would look something like `PATCH /articles/{ArticleId}` with a body containing a reference to `Alice`.

First to all, we wish to authorize this operation by the verifying permissions related to the resources that are **explicity affected**  by it:
1. Is the API user allowed to update `Article`?
2. Is the API user allowed to update `Alice`?

Apart from this, we also wish to verify permissions for the resources that are **implicitly affected** by this operation: `Bob` and `Old Article`. Setting `Alice` as the new author of `Article` will result in removing the following two relationships:  `Bob` being an author of `Article`, and `Alice` being an author of  `Old Article`. Therefore, we wish wish to verify the related permissions:

3. Is the API user allowed to update `Bob`?
4. Is the API user allowed to update `Old Article`?

This authorization requirement can be fulfilled as follows.

For checking the permissions for the explicitly affected resources, `Article` and `Alice`, we may implement the `BeforeUpdate` hook for `Article`:

```c#
public override IEnumerable<Article> BeforeUpdate(IResourceDiff<Article> entityDiff,
    ResourcePipeline pipeline)
{
    if (pipeline == ResourcePipeline.Patch)
    {
        Article article = entityDiff.RequestEntities.Single();

        if (!_auth.CanEditResource(article))
        {
            throw new JsonApiException(403, "Forbidden to update properties of this article",
                new UnauthorizedAccessException());
        }

        if (entityDiff.GetByRelationship<Person>().Any() &&
            _auth.CanEditRelationship<Person>(article))
        {
            throw new JsonApiException(403, "Forbidden to update relationship of this article",
                new UnauthorizedAccessException());
        }
    }

    return entityDiff.RequestEntities;
}
```

and the `BeforeUpdateRelationship` hook for `Person`:

```c#
public override IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids,
    IAffectedRelationships<Person> resourcesByRelationship, ResourcePipeline pipeline)
{
    var updatedOwnerships = resourcesByRelationship.GetByRelationship<Article>();

    if (updatedOwnerships.Any())
    {
        Person person =
            resourcesByRelationship.GetByRelationship<Article>().Single().Value.First();

        if (_auth.CanEditRelationship<Article>(person))
        {
            throw new JsonApiException(403, "Forbidden to update relationship of this person",
                new UnauthorizedAccessException());
        }
    }

    return ids;
}
```

To verify the permissions for the implicitly affected resources, `Old Article` and `Bob`, we need to implement the `BeforeImplicitUpdateRelationship` hook for `Article`:

```c#
public override void BeforeImplicitUpdateRelationship(
    IAffectedRelationships<Article> resourcesByRelationship, ResourcePipeline pipeline)
{
    var updatedOwnerships = resourcesByRelationship.GetByRelationship<Person>();

    if (updatedOwnerships.Any())
    {
        Article article =
            resourcesByRelationship.GetByRelationship<Person>().Single().Value.First();

        if (_auth.CanEditRelationship<Person>(article))
        {
            throw new JsonApiException(403, "Forbidden to update relationship of this article",
                new UnauthorizedAccessException());
        }
    }
}
```

and similarly for `Person`:

```c#
public override void BeforeImplicitUpdateRelationship(
    IAffectedRelationships<Person> resourcesByRelationship, ResourcePipeline pipeline)
{
    var updatedOwnerships = resourcesByRelationship.GetByRelationship<Article>();
    if (updatedOwnerships.Any())
    {
        Person person =
            resourcesByRelationship.GetByRelationship<Article>().Single().Value.First();

        if (_auth.CanEditRelationship<Article>(person))
        {
            throw new JsonApiException(403, "Forbidden to update relationship of this article",
                new UnauthorizedAccessException());
        }
    }
}
```

## Using Resource Hooks without Entity Framework Core

If you want to use Resource Hooks without Entity Framework Core, there are several things that you need to consider that need to be met. For any resource that you want to use hooks for:
1. The corresponding resource repository must fully implement `IResourceReadRepository<TEntity, TId>`
2. If you are using custom services, you will be responsible for injecting the `IResourceHookExecutor` service into your services and call the appropriate methods. See the [hook execution overview](#4-hook-execution-overview) to determine which hook should be fired in which scenario.

If you are required to use the `BeforeImplicitUpdateRelationship` hook (see previous example), there is an additional requirement. For this hook, given a particular relationship, JsonApiDotNetCore needs to be able to resolve the inverse relationship. For example: if `Article` has one  author (a `Person`), then it needs to be able to resolve the `RelationshipAttribute` that corresponds to the inverse relationship for the `author` property. There are two approaches :

1. **Tell JsonApiDotNetCore how to do this only for the relevant models**.  If you're using the `BeforeImplicitUpdateRelationship` hook only for a small set of models, eg only for the relationship of the example, then it is easiest to provide the `inverseNavigationProperty` as follows:

```c#
public class Article : Identifiable
{
    [HasOne("author", InverseNavigationProperty: "OwnerOfArticle")]
    public Person Author { get; set; }
}

public class Person : Identifiable
{
    [HasOne("article")]
    public Article OwnerOfArticle { get; set; }
}
```

2. **Tell JsonApiDotNetCore how to do this in general**. For full support, you can provide JsonApiDotNetCore with a custom service implementation of the `IInverseNavigationResolver` interface. relationship of the example, then it is easiest to provide the `InverseNavigationProperty` as follows:

```c#
public class CustomInverseNavigationResolver : IInverseNavigationResolver
{
    public void Resolve()
    {
        // the implementation of this method depends completely on
        // the data access layer you're using.
        // It should set the RelationshipAttribute.InverseNavigationProperty property
        // for all (relevant) relationships.
        // To have an idea of how to implement this method, see the InverseNavigationResolver class
        // in the source code of JsonApiDotNetCore.
    }
}
```

This service will then be run once at startup and take care of the metadata that is required for `BeforeImplicitUpdateRelationship` to be supported.

*Note: don't forget to register this singleton service with the service provider.*


## Synchronizing data across microservices
If your application is built using a microservices infrastructure, it may be relevant to propagate data changes between microservices, [see this article for more information](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/integration-event-based-microservice-communications). In this example, we will assume the implementation of an event bus and we will publish data consistency integration events using resource hooks.

```c#
public class ArticleResource : ResourceHooksDefinition<Article>
{
    private readonly IEventBus _bus;
    private readonly IJsonApiContext _context;

    public ArticleResource(IEventBus bus, IJsonApiContext context)
    {
        _bus = bus;
        _context = context;
    }

    public override void AfterCreate(HashSet<Article> entities, ResourcePipeline pipeline)
    {
        foreach (var article in entities )
        {
            var @event = new ResourceCreatedEvent(article);
            _bus.Publish(@event);
        }
    }

    public override void AfterDelete(HashSet<Article> entities, ResourcePipeline pipeline,
        bool succeeded)
    {
        foreach (var article in entities)
        {
            var @event = new ResourceDeletedEvent(article);
            _bus.Publish(@event);
        }
    }

    public override void AfterUpdate(HashSet<Article> entities, ResourcePipeline pipeline)
    {
        foreach (var article in entities)
        {
            // You could inject ITargetedFields and use it to pass along
            // only the attributes that were updated

            var @event = new ResourceUpdatedEvent(article,
                properties: _targetedFields.Attributes);

            _bus.Publish(@event);
        }
    }
}
```

## Hooks for many-to-many join tables
In this example we consider an application with a many-to-many relationship: `Article` and `Tag`, with an internally used `ArticleTag` join-type.

Usually, join table records will not contain any extra information other than that which is used internally for the many-to-many relationship. For this example, the join-type should then look like:

```c#
public class ArticleTag
{
    public int ArticleId { get; set; }
    public Article Article { get; set; }

    public int TagId { get; set; }
    public Tag Tag { get; set; }
}
```

If we then eg. implement the `AfterRead` and `OnReturn` hook for `Article` and `Tag`, and perform a `GET /articles?include=tags` request, we may expect the following order of execution:

1. Article AfterRead
2. Tag AfterRead
3. Article OnReturn
4. Tag OnReturn

Note that under the hood, the *join table records* (instances of `ArticleTag`) are also being read, but we did not implement any hooks for them. In this example, for these records, there is little relevant business logic that can be thought of.

Sometimes, however, relevant data may be stored in the join table of a many-to-many relationship. Let's imagine we wish to add a property `LinkDate` to the join table that reflects when a tag was added to an article. In this case, we may want to execute business logic related to these records: we may for example want to hide any tags that were added to an article longer than 2 weeks ago.

In order to achieve this, we need to change `ArticleTag` to `ArticleTagWithLinkDate` as follows:

```c#
public class ArticleTagWithLinkDate : Identifiable
{
    public int ArticleId { get; set; }

    [HasOne("Article")]
    public Article Article { get; set; }

    public int TagId { get; set; }

    [HasOne("Tag")]
    public Tag Tag { get; set; }

    public DateTime LinkDate { get; set; }
}
```

Then, we may implement a hook for `ArticleTagWithLinkDate` as usual:

```c#
public class ArticleTagWithLinkDateResource : ResourceHooksDefinition<ArticleTagWithLinkDate>
{
    public override IEnumerable<ArticleTagWithLinkDate> OnReturn(
        HashSet<ArticleTagWithLinkDate> entities, ResourcePipeline pipeline)
    {
        return entities.Where(article => (DateTime.Now - article.LinkDate) < 14);
    }
}
```

Then, for the same request `GET /articles?include=tags`, the order of execution of the hooks will look like:
1. Article AfterRead
2. Tag AfterRead
3. Article OnReturn
4. ArticleTagWithLinkDate OnReturn
5. Tag OnReturn

And the included collection of tags per article will only contain tags that were added less than two weeks ago.

Note that the introduced inheritance and added relationship attributes does not further affect the many-to-many relationship internally.

# 4. Hook execution overview


This table below shows the involved hooks per pipeline.
<table>
  <tr>
    <th rowspan="2">Pipeline</th>
    <th colspan="5"><span style="font-style:italic">Execution Flow</span></th>
  </tr>
  <tr>
    <td align="center"><b>Before Hooks</b></td>
    <td align="center" colspan="2"><b>Repository Actions</td>
    <td align="center"><b>After Hooks</td>
    <td align="center"><b>OnReturn</td>
  </tr>
  <tr>
    <td>Get</td>
    <td align="center">BeforeRead</td>
    <td align="center" colspan="2" rowspan="3">read</td>
    <td align="center">AfterRead</td>
    <td align="center">[x]</td>
  </tr>
  <tr>
    <td>GetSingle</td>
    <td align="center">BeforeRead</td>
    <td align="center">AfterRead</td>
    <td align="center">[x]</td>
  </tr>
  <tr>
    <td>GetRelationship</td>
    <td align="center">BeforeRead</td>
    <td align="center">AfterRead</td>
    <td align="center">[x]</td>
  </tr>
  <tr>
    <td>Post</td>
    <td align="center">BeforeCreate</td>
    <td align="center" colspan="2">create<br>update relationship</td>
    <td align="center">AfterCreate</td>
    <td align="center">[x]</td>
  </tr>
  <tr>
    <td>Patch</td>
    <td align="center">BeforeUpdate<br>BeforeUpdateRelationship<br>BeforeImplicitUpdateRelationship</td>
    <td align="center" colspan="2">update<br>update relationship<br>implicit update relationship</td>
    <td align="center">AfterUpdate<br>AfterUpdateRelationship</td>
    <td align="center">[x]</td>
  </tr>
  <tr>
    <td>PatchRelationship</td>
    <td align="center">BeforeUpdate<br>BeforeUpdateRelationship</td>
    <td align="center" colspan="2">update<br>update relationship<br>implicit update relationship</td>
    <td align="center">AfterUpdate<br>AfterUpdateRelationship</td>
    <td align="center">[ ]</td>
  </tr>
  <tr>
    <td>Delete</td>
    <td align="center">BeforeDelete</td>
    <td align="center" colspan="2">delete<br>implicit update relationship</td>
    <td align="center">AfterDelete</td>
    <td align="center">[ ]</td>
  </tr>
  <tr>
    <td>BulkPost</td>
    <td colspan="5" align="center"><i>Not yet supported</i></td>
  </tr>
  <tr>
    <td>BulkPatch</td>
    <td colspan="5" align="center"><i>Not yet supported</i></td>
  </tr>
  <tr>
    <td>BulkDelete</td>
    <td colspan="5" align="center"><i>Not yet supported</i></td>
  </tr>
</table>
