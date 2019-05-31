
# Resource Hooks
This section covers the usage of **Resource Hooks**, which is a feature of`ResourceDefinition<T>`. See the [ResourceDefinition usage guide](resource-definitions.md) for a general explanation on how to set up a `ResourceDefinition<T>`. For a quick start, jump right to the [Getting started: most minimal example](#getting-started-most-minimal-example) section.

By implementing resource hooks on a `ResourceDefintion<T>`, it is possible to intercept the execution of the **Resource Service Layer** (RSL) in various ways. This enables the developer to conveniently define business logic without having to override the RSL. It can be used to implement e.g.
* Authorization
* [Event-based synchronisation between microservices](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/integration-event-based-microservice-communications)
* Logging 
* Transformation of the served data

This usage guide covers the following sections
1.  [**Semantics: pipelines, actions and hooks**](#semantics-pipelines-actions-and-hooks).
Understanding the semantics will be helpful in identifying which hooks on `ResourceDefinition<T>` you need to implement for your use-case.
2.  [**Basic usage**](#basic-usage)
      * [**Getting started: most minimal example**](#getting-started-most-minimal-example)
      * [**Logging**](#logging)
      * [**Transforming data with OnReturn**](#transforming-data-with-onreturn)
      * [**Loading database values**](#loading-database-values)
3.  [**Advanced usage**](#advanced-usage)
      * [**Simple authorization: explicitly affected resources**](#simple-authorization-explicitly-affected-resources)
      * [**Advanced authorization: implicitly affected resources**](#advanced-authorization-implicitly-affected-resources)
      * [**Synchronizing data across microservices**](#synchronizing-data-across-microservices)
      * [**Hooks for many-to-many join tables**](#hooks-for-many-to-many-join-tables)
4.  [**Hook execution overview**](#hook-execution-overview)
  A table overview of all pipelines and involved hooks

# 1. Semantics: pipelines, actions and hooks

## Pipelines
The different execution flows within the RSL that may be intercepted can be identified as **pipelines**. Examples of such pipelines are
* **Post**: creation of a resource (triggered by the endpoint `POST /my-resource`).
* **PostBulk**: creation of multiple resources (triggered by the endpoint `POST /bulk/my-resource`).
  * *NB: hooks are not yet supported with bulk operations.* 
* **Get**: reading a resource (triggered by the endpoint `GET /my-resource`).
* **GetSingle**: reading a single resource (triggered by the endpoint `GET /my-resource/1`).

See the [ResourcePipeline](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/feat/%23477/src/JsonApiDotNetCore/Hooks/Execution/ResourcePipelineEnum.cs) enum for a full list of available pipelines.

## Actions
Each pipeline is associated with a set of **actions** that work on resources and their relationships. These actions reflect the associated database operations that is performed by JsonApiDotNetCore (in the Repository Layer). Typically, the RSL will execute some service-layer-related code, then invoke the Repository Layer which will perform these actions, after which the execution returns to the RSL. 

Note that some actions are shared across different pipelines, and note that most pipelines perform multiple actions.  There are two types of actions: **main resource actions** and **nested resource actions**. 

### Main resource actions
Most actions are trivial in the context of the pipeline where they're executed from. They may be recognised as the familiar *CRUD* operations of an API. These actions are:

* The `create` action: the **Post** pipeline will `create` a resource
* The `read` action: the **Get** and  **GetSingle** pipeline will `read` (a) resource(s).
* The `update` action: the **Patch** pipeline will `update` a resource.
* The `delete` action: the **Delete** pipeline will `delete` a resource.

These actions are called the **main resource actions** of a particular pipeline because **they act on the request resource**. For example, when an `Article` is created through the **Post** pipeline, its main action, `create`, will work on that `Article`.

### Nested Resource Actions
Some other actions might be overlooked, namely the nested resource actions. These actions are

*  `update relationship`  for directly affected relationships
*  `implicit update relationship` for implicitly affected relationships
* `read` for included relationships

These actions are called **nested resource actions** of a particular pipeline because **they act on involved (nested) resources** instead of the main request resource. For example, when loading articles and their respective authors (`GET /articles?include=author`), the `read` action on `Article` is the main action, and the `read` action on `Person` is the nested action.

**The `update relationship` action**
[As per the Json:Api specification](https://jsonapi.org/format/#crud-creating](https://jsonapi.org/format/#crud-creating), the **Post** pipeline also allows for an `update relationship` action on an already existing resource. For example, when creating an `Article` it is possible to simultaneously relate it to an existing `Person` by setting its author. In this case, the `update relationship` action is a nested action that will work on that `Person`.

**The `implicit update relationship` action** 
the **Delete**  pipeline also allows for an `implicit update relationship` action on an already existing resource. For example, for an  `Article` that its author property assigned to a particular `Person`,  the relationship between them is destroyed when this article is deleted. This update is "implicit" in the sense that no explicit information about that `Person` was provided in the request that triggered this pipeline. An `implicit update relationship` action is therefore performed on that `Person`.  See [this section](#advanced-authorization-implicitly-affected-resources) for a more detailed.

### Shared actions
Note that **some actions are shared across pipelines**. For example, both the **Post** and **Patch** pipeline can perform the `update relationship`  action on an (already existing) involved resource. Similarly, the **Get** and **GetSingle** pipelines perform the same `read` action.
<br><br>
For a complete list of actions associated with each pipeline, see the [overview table](#hook-execution-overview).

## Hooks
For all actions it is possible to implement **at least one hook** to intercept its execution. These hooks can be implemented by overriding the corresponding virtual  implementation on `ResourceDefintion<T>`. (Note that the base implementation is a dummy implementation, which is ignored when firing hooks.)

### Action related hooks
As an example, consider the `create` action for the `Article` Resource. This action can be intercepted by overriding the
*  `ResourceDefintion<Article>.BeforeCreate` hook for custom logic **just before** execution of the main `create` action
* `ResourceDefintion<Article>.AfterCreate` hook for custom logic **just after** execution of the main `create` action

If with the creation of an `Article`  a relationship to `Person` is updated simultaneously, this can be intercepted by overriding the
* `ResourceDefintion<Person>.BeforeUpdateRelationship` hook for custom logic **just before** the execution of the nested `update relationship` action.
* `ResourceDefintion<Person>.AfterUpdateRelationship` hook for custom logic **just after** the execution of the nested `update relationship` action.

### OnReturn hook
As mentioned in the previous section, some actions are shared across hooks. One of these actions is the `return` action. Although not strictly compatible with the *CRUD* vocabulary, and although not executed by the Repository Layer, pipelines are also said to perform a `return` action when any content is to be returned from the API. For example, the **Delete** pipeline does not return any content, but a *HTTP 204 No Content* instead, and will therefore not perform a `return` action. On the contrary, the **Get** pipeline does return content, and will therefore perform a `return action`

Any return content can be intercepted and transformed as desired by implementing the `ResourceDefintion<TEntity>.OnReturn` hook which intercepts the `return` action. For this action, there is no distinction between a `Before` and `After` hook, because no code after a `return` statement can be evaluated. Note that the `return` action can work on *main resources as well as nested resources*, see  [this example below](#transforming-data-with-onreturn).
<br><br>
For an overview of all pipelines, hooks and actions, see the table below, and for more detailed information about the available hooks, see the [IResourceHookContainer<T>](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/ab1f96d8255532461da47d290c5440b9e7e6a4a5/src/JsonApiDotNetCore/Hooks/IResourceHookContainer.cs) interface.

# 2. Basic usage

## Getting started: most minimal example
To use resource hooks, you are required to turn them on in your `startup.cs` configuration

```c#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddJsonApi<ApiDbContext>(
        options =>
        {
            options.EnableResourceHooks = true; // default is false
            options.LoadDatabaseValues = false; // default is true
        }
    );
    ...
}
```
For this example, we may set `LoadDatabaseValues` to `false`. See the [Loading database values](#loading-database-values) example for more information about this option.

The simplest case of resource hooks we can then implement should not require a lot of explanation.  This hook would triggered by any default JsonApiDotNetCore API route for `Article`.
```c#
public class ArticleResource : ResourceDefinition<Article>
{
    public override IEnumerable<Article> OnReturn(HashSet<Article> entities, ResourcePipeline pipeline)
    {
        Console.WriteLine("This hook does not do much apart from writing this message" +
             " to the console just before serving the content");
        return entities;
    }
}
```
## Logging
This example shows how some actions can be logged on the level of API users. 

First consider the following scoped service which creates a logger bound to a particular user and request.
```c#
/// This is a scoped service, which means wil log will have a request-based
/// unique id associated to it.
public class UserActionsLogger : IUserActionsLogger
{
    public ILogger Instance { get; private set; }
    public UserActionsLogger(ILoggerFactory loggerFactory,
                             IUserService userService)
    {
        var userId = userService.GetUser().Id;
        Instance = loggerFactory.CreateLogger($"[request: {Guid.NewGuid()}" 
        + "user: {userId}]");
    }
}
```
Now, let's assume our application has two resources: `Article` and `Person`, and that there exist a one-to-one  and one-to-many relationship between them (`Article has one Author` and `Article has many Reviewers`). Let's assume we are required to log the following events
* An API user deletes an article
* An API user removes the `Author` relationship of a person
* An API user removes the `Reviewer` relationship of a person

This could be achieved in the following way
```c#
/// Note that resource definitions are also registered as scoped services.
public class ArticleResource : ResourceDefinition<Article>
{
    private readonly ILogger _userLogger;
    public ArticleResource(IUserActionsLogger logService)
    {
        _userLogger = logService.Instance;
    }

    public override void AfterDelete(HashSet<Article> entities, ResourcePipeline pipeline, bool succeeded)
    {
        if (!succeeded) return
        foreach (Article a in entities)
        {
            _userLogger.Log(LogLevel.Information, $"Deleted article '{a.Name}' with id {a.Id}");
        }
    }
}

public class PersonResource : ResourceDefinition<Person>
{
    private readonly ILogger _userLogger;
    public PersonResource(IUserActionsLogger logService)
    {
        _userLogger = logService.Instance;
    }

    public override void AfterUpdateRelationship(IUpdatedRelationshipHelper<Person> relationshipHelper, ResourcePipeline pipeline)
    {
      var updatedRelationshipsToArticle = relationshipHelper.EntitiesRelatedTo<Article>();
        foreach (var updated in updatedRelationshipsToArticle)
        {
            RelationshipAttribute relationship = updated.Key;
            HashSet<Person> affectedEntities = updated.Value;

            foreach (Person p in affectedEntities)
            {
                if (pipeline == ResourcePipeline.Delete)
                {
                    _userLogger.Log(LogLevel.Information, $"Deleted the {relationship.PublicRelationshipName}" + 
                    "relationship to Article for person '{p.FirstName + p.LastName}' with {p.Id}");
                }
            }
        }
    }
}
```

If eg. a API user deletes an article with title *JSON:API paints my bikeshed!* that had related as author *John* and as reviewer *Frank*, the logs generated logs would look something like

```
[request: 186190e3-1900-4329-9181-42082258e7b4, user: dd1cd99d-60e9-45ca-8d03-a0330b07bdec] Deleted article 'JSON:API paints my bikeshed!' with id fac0436b-7aa5-488e-9de7-dbe00ff8f04d
[request: 186190e3-1900-4329-9181-42082258e7b4, user: dd1cd99d-60e9-45ca-8d03-a0330b07bdec] Deleted the author relationship to Article for person 'John' with id 2ec3990d-c816-4d6d-8531-7da4a030d4d0
[request: 186190e3-1900-4329-9181-42082258e7b4, user: dd1cd99d-60e9-45ca-8d03-a0330b07bdec] Deleted the reviewer relationship to Article for person 'Frank' with id 42ad6eb2-b813-4261-8fc1-0db1233e665f
```
## Transforming data with OnReturn
Using the `OnReturn` hook, any set of resources can be manipulated as desired before serving it from the API. One of the use-cases for this is being able to perform a [filtered included](https://github.com/aspnet/EntityFrameworkCore/issues/1833), which is currently not supported by Entity Framework Core.

As an example, consider again an application with the `Article`  and `Person` resource, and let's assume the following business rules
* when reading `Article`s, we never want to show articles for which the `SoftDeleted` property is set to true.
* when reading `Person`s, we never want to show people who wish to remain anonymous (`Anonymous` is set to true).

This can be achieved as follows.

```c#
public class ArticleResource : ResourceDefinition<Article>
{
    public override IEnumerable<Article> OnReturn(HashSet<Article> entities, ResourcePipeline pipeline)
    {
        return entities.Where(a => a.SoftDeleted == false);
    }
}

public class PersonResource : ResourceDefinition<Person>
{
    public override IEnumerable<Person> OnReturn(HashSet<Person> entities, ResourcePipeline pipeline)
    {
        if (pipeline == ResourcePipeline.Get)
        {
            return entities.Where(p => p.Anonymous == false);
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
 
For example, consider a scenario in with the following two requirements: 
* We need to log all updates on resources revealing their old and new value.
* We need to check if the property `IsLocked` is set is `true`, and if so, cancel the operation.
  
 Consider an `Article` with title *Hello there* and API user trying to update the the title of this article to *Bye bye*.  The above requirements could be implemented as follows
```c#
public class ArticleResource : ResourceDefinition<Article>
{
    private readonly ILogger _logger;
    private readonly IJsonApiContext _context;
    public constructor ArticleResource(ILogger logger, IJsonApiContext context)
    {
        _logger = logger;
        _context = context;  
    } 

    public override IEnumerable<Article> BeforeUpdate(EntityDiff<Article> entityDiff, ResourcePipeline pipeline)
    {
        // PropertyGetter is a helper class that takes care of accessing the values on an instance of Article using reflection.
        var getter = new PropertyGetter<Article>();

        entityDiff.RequestEntities.ForEach( requestEntity => 
        {

            var databaseEntity = entityDiff.DatabaseEntities.Single( e => e.Id == a.Id);

            if (databaseEntity.IsLocked) throw new JsonApiException(403, "Forbidden: this article is locked!")

            foreach (var attr in _context.AttributesToUpdate)
            {
                var oldValue = getter(databaseEntity, attr);
                var newValue = getter(requestEntity, attr);

                _logger.LogAttributeUpdate(oldValue, newValue)
            }
        });
        return entityDiff.RequestEntities;
    }
}
```

Note that database values are turned on by default. They can be turned of globally by configuring the startup as follows:
```c#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddJsonApi<ApiDbContext>(
        options =>
        {
            options.LoadDatabaseValues = false; // default is true
        }
    );
    ...
}
```

The global setting can be used together with toggling the option on and off on the level of individual hooks using the `LoadDatabaseValues` attribute:
```c#
public class ArticleResource : ResourceDefinition<Article>
{
  [LoadDatabaseValues(true)]
    public override IEnumerable<Article> BeforeUpdate(EntityDiff<Article> entityDiff, ResourcePipeline pipeline)
    {
      ....
  }
  
  [LoadDatabaseValues(false)] 
    public override IEnumerable<string> BeforeUpdateRelationships(HashSet<string>  ids,  IAffectedRelationships<Article>  resourcesByRelationship,  ResourcePipeline  pipeline)
    {
      // the entities stored in the IAffectedRelationships<Article> instance 
      // are plain resource identifier objects when LoadDatabaseValues is turned off,
      // or objects loaded from the database when LoadDatabaseValues is turned on.
     ....
  }
}
```

Note that there are some hooks that the  `LoadDatabaseValues` option and attribute does not affect. The only hooks that are affected are:
* `BeforeUpdate`
* `BeforeUpdateRelationship`



# 3. Advanced usage

## Simple authorization: explicitly affected resources
Resource hooks can be used to easily implement authorization in your application.  As an example, consider the case in which an API user is not allowed to see anonymous people, which is reflected by the `Anonymous` property on `Person`  being set to true`true`.  The API should handle this as follows:
* When reading people (`GET /people`), it should hide all people that are set to anonymous.
* When reading a single person (`GET /people/{id}`), it should throw an authorization error if the particular requested person is set to anonymous.

This can be achieved as follows:
```c#
public class PersonResource : ResourceDefinition<Person>
{ 
  private readonly _IAuthorizationHelper _auth;
    public constructor PersonResource(IAuthorizationHelper auth)
    {
      // IAuthorizationHelper is a helper service that handles all authorization related logic.
      _auth = auth;
    }

    public override IEnumerable<Person> OnReturn(HashSet<Person> entities, ResourcePipeline pipeline)
    {
    if (!_auth.CanSeeSecretPeople()) 
    {
       if (pipeline == ResourcePipeline.GetSingle) 
       {
         throw new JsonApiException(403, "Forbidden to view this person", new  UnauthorizedAccessException());
       } 
       entities = entities.Where( p => !p.IsSecret)
    }
    return entities;
    }
}
```
This example of authorization is considered simple because it only involves one resource. The next example shows a more complex case

## Advanced authorization: implicitly affected resources
Let's consider an authorization scenario for which we are required to implement multiple hooks across multiple resource definitions. We will assume the following:
* There exists a one-to-one relationship between `Article` and `Person`: an article can have only one author, and a person can be author of only one article.
* The author of article `Old Article` is person `Alice`.
* The author of article `New Article` is person `Bob`.

Now let's consider an API user that tries to update `New Article` by setting its author to `Alice`. First to all, we wish to authorize this operation by the verifying permissions related to the resources that are **explicity affected**  by it:
1. Is the API user allowed to update `New Article`?
2. Is the API user allowed to update `Alice`?

Apart from this, we also wish to verify permissions for the resources that are **implicitly affected** by this operation: `Bob` and `Old Article`. Setting `Alice` as the new author of `New Article` will result in removing the following two relationships:  `Bob` being an author of `New Article`, and `Alice` being an author of  `Old Article`. Therefore, we wish wish to verify the related permissions:
1. Is the API user allowed to update `Bob`?
2. Is the API user allowed to update `Old Article`?

This authorization requirement can be fulfilled as follows. 

For checking the permissions for the explicitly affected resources, `New Article` and `Alice`, we may implement the `BeforeUpdate` hook for `Article`:
```c#
public override IEnumerable<Article> BeforeUpdate(EntityDiff<Article> entityDiff, ResourcePipeline pipeline)
{
    if (pipeline == ResourcePipeline.Patch)
    {
        Article a = entityDiff.RequestEntities.Single();
        if (!_auth.CanEditResource(a))
        {
            throw new JsonApiException(403, "Forbidden to update properties of this article", new UnauthorizedAccessException());
        }
        if (entityDiff.GetByRelationship<Person>().Any() && _auth.CanEditRelationship<Person>(a))
        {
            throw new JsonApiException(403, "Forbidden to update relationship of this article", new UnauthorizedAccessException());
        }
    }
    return entityDiff.RequestEntities;
}
```

 and the `BeforeUpdateRelationship` hook for `Person`:
```c#
public override IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IAffectedRelationships<Person> resourcesByRelationship, ResourcePipeline pipeline) 
{
    var updatedOwnerships = resourcesByRelationship.GetByRelationship<Article>();
    if (updatedOwnerships.Any())
    {
        Person p = resourcesByRelationship.GetByRelationship<Article>().Single().Value.First();
        if (_auth.CanEditRelationship<Article>(p))
        {
            throw new JsonApiException(403, "Forbidden to update relationship of this person", new UnauthorizedAccessException());
        }
    }
    return ids; 
}
```

To verify the permissions for the implicitly affected resources, `Old Article` and `Bob`, we need to implement the `BeforeImplicitUpdateRelationship` hook for `Article`:
```c#
public override void BeforeImplicitUpdateRelationship(IAffectedRelationships<Article> resourcesByRelationship, ResourcePipeline pipeline)
{
    var updatedOwnerships = resourcesByRelationship.GetByRelationship<Person>();
    if (updatedOwnerships.Any())
    {
        Article a = resourcesByRelationship.GetByRelationship<Person>().Single().Value.First();
        if (_auth.CanEditRelationship<Person>(a))
        {
            throw new JsonApiException(403, "Forbidden to update relationship of this article", new UnauthorizedAccessException());
        }
    }
}
```
and similarly for `Person`: 
```c#
public override void BeforeImplicitUpdateRelationship(IAffectedRelationships<Person> resourcesByRelationship, ResourcePipeline pipeline)
{
    var updatedOwnerships = resourcesByRelationship.GetByRelationship<Article>();
    if (updatedOwnerships.Any())
    {
        Person p = resourcesByRelationship.GetByRelationship<Article>().Single().Value.First();
        if (_auth.CanEditRelationship<Article>(p))
        {
            throw new JsonApiException(403, "Forbidden to update relationship of this article", new UnauthorizedAccessException());
        }
    }
}
```
## Synchronizing data across microservices
If your application is built using a microservices infrastructure, it may be relevant to propagate data changes between microservices, [see this article for more information](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/integration-event-based-microservice-communications). In this example, we will assume the implementation of an event bus and we will publish data consistency integration events using resource hooks.

```c#
public class ArticleResource : ResourceDefinition<Article>
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

    public override void AfterDelete(HashSet<Article> entities, ResourcePipeline pipeline, bool succeeded)
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
          // You could inject IJsonApiContext and use it to pass along only the attributes that were updated
            var @event = new ResourceUpdatedEvent(article, properties: _context.AttributesToUpdate);
            _bus.Publish(@event);
        }
    }
}
```

## Hooks for many-to-many join tables
In this example we consider an application with a many-to-many relationships: `Article` and `Tag`, with an internally used `ArticleTag` throughtype.

Usually, join table records will not contain any extra information other than that which is used internally for the many-to-many relationship. For this example, the throughtype should then look like:
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
public class ArticleTagWithLinkDateResource : ResourceDefinition<ArticleTagWithLinkDate>
{
    public override IEnumerable<ArticleTagWithLinkDate> OnReturn(HashSet<ArticleTagWithLinkDate> entities, ResourcePipeline pipeline)
    {
        return entities.Where(e => (DateTime.Now - e.LinkDate) < 14);
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
    <td align="center">✅</td>
  </tr>
  <tr>
    <td>GetSingle</td>
    <td align="center">BeforeRead</td>
    <td align="center">AfterRead</td>
    <td align="center">✅</td>
  </tr>
  <tr>
    <td>GetRelationship</td>
    <td align="center">BeforeRead</td>
    <td align="center">AfterRead</td>
    <td align="center">✅</td>
  </tr>
  <tr>
    <td>Post</td>
    <td align="center">BeforeCreate</td>
    <td align="center" colspan="2">create<br>update relationship</td>
    <td align="center">AfterCreate</td>
    <td align="center">✅</td>
  </tr>
  <tr>
    <td>Patch</td>
    <td align="center">BeforeUpdate<br>BeforeUpdateRelationship<br>BeforeImplicitUpdateRelationship</td>
    <td align="center" colspan="2">update<br>update relationship<br>implicit update relationship</td>
    <td align="center">AfterUpdate<br>AfterUpdateRelationship</td>
    <td align="center">✅</td>
  </tr>
  <tr>
    <td>PatchRelationship</td>
    <td align="center">BeforeUpdate<br>BeforeUpdateRelationship</td>
    <td align="center" colspan="2">update<br>update relationship<br>implicit update relationship</td>
    <td align="center">AfterUpdate<br>AfterUpdateRelationship</td>
    <td align="center">❌</td>
  </tr>
  <tr>
    <td>Delete</td>
    <td align="center">BeforeDelete</td>
    <td align="center" colspan="2">delete<br>implicit update relationship</td>
    <td align="center">AfterDelete</td>
    <td align="center">❌</td>
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
