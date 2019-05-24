
# Resource Hooks
This section covers the usage of **Resource Hooks**, which is a feature of`ResourceDefinition<T>`. See the [ResourceDefinition usage guide](resource-definitions.md) for a general explanation on how to set up a `ResourceDefinition<T>`.

By implementing resource hooks on a `ResourceDefintion<T>`, it is possible to intercept the execution of the **Resource Service Layer** (RSL) in various ways. This enables the developer to conveniently declare business logic without having to override the RSL, which can be leveraged to implement e.g.
* Authorization
* [Event-based synchronisation between microservices](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/integration-event-based-microservice-communications)
* Logging 
* Transformation of the served data

This usage guide covers the following sections
1.  [**Semantics: pipelines, actions and hooks**](#semantics-pipelines-actions-and-hooks).
Understanding the semantics will be helpful in identifying which hooks on `ResourceDefinition<T>` you need to implement for your use-case.
2.  [**Hook execution overview**](#hook-execution-overview)
A table overview of all pipelines and involved hooks
3.  [**Examples: basic usage**](#examples-basic-usage)
     1. [**Most minimal example**](#most-minimal-example)
     2. [**Logging**](#logging)
     3. [**Transforming data with OnReturn**](#transforming-data-with-onreturn)
5.  [**Examples: advanced usage**](#examples-advanced-usage)
     1. [**Simple authorization: explicitly affected resources**](#simple-authorization-explicitly-affected-resources)
     2. [**Advanced authorization: implicitly affected resources**](#simple-authorization-implicitly-affected-resources)
     3. [**Sychronizing data across microservices**](#sychronizing-data-across-microservices)
     4. [**Hooks for many-to-many join tables**](#hooks-for-many-to-many-join-tables)


## Simple Authorization: explicit usage of resources
Advanced Authorization: using implicit updated relationships

todo: 
LoadDatabaseValues
EnableHooks
Implicit vs non implicit explanation
Resource Hooks have been introduced in v4.0.

# 1. Semantics: pipelines, actions and hooks

## Pipelines
The different execution flows within the RSL that may be intercepted can be identified as **pipelines**. Examples of such pipelines are
* **Post**: creation of a resource (triggered by the endpoint `POST /my-resource`).
* **PostBulk**: creation of multiple resources (triggered by the endpoint `POST /bulk/my-resource`).
  * *NB: hooks are not yet supported with bulk operations.* 
* **Get**: reading a resource (triggered by the endpoint `GET /my-resource`).
* **GetSingle**: reading a single resource (triggered by the endpoint `GET /my-resource/1`).

See the [ResourcePipeline](www.will-add-link-later.com) enum for a full list of available pipelines.

## Actions
Each pipeline is associated with a set of **actions** that work on resources and their relationships. These actions represent the associated database operations that JsonApiDotNetCore performs in the Repository Layer. Typically, the RSL will execute some service-layer-related code, then invoke the Repository Layer which will perform these actions, after which the execution returns to the RSL. Note that some actions are shared across different pipelines, and note that most pipelines perform multiple actions. 

There are two types of actions: **main resource actions** and **nested resource actions**. 

### Main Resource Actions
Most actions are trivial in the context of the pipeline where they're executed from. They may be recognised as the standard *CRUD* operations of an API. These actions are:

* The `create` action: the **Post** pipeline will `create` a resource
* The `read` action: The **Get** and  **GetSingle** pipeline will `read` (a) resource(s).
* The `update` action: The **Patch** pipeline will `update` a resource.
* The `delete` action: The **Delete** pipeline will `delete` a resource.

These actions are called the **main resource actions** of a particular pipeline because **they act on the request resource**. For example, when an `Article` is created through **Post** pipeline, its main action, `create`, will work on that `Article`.

### Nested Resource Actions
Some other actions might be overlooked, namely the nested resource actions. These actions are

*  `update relationship`  for directly affected relationships
*  `implicit update relationship` for implicitly affected relationships

These actions are called **nested resource actions** of a particular pipeline because **they act on involved (nested) resources** instead of the main request resource.

**The `update relationship` action**. 
The **Post** pipeline also allows for an `update relationship` action on an already existing resource, [see the spec]([https://jsonapi.org/format/#crud-creating](https://jsonapi.org/format/#crud-creating)). Eg. when creating an `Article` while simultaneously relating it to an existing `Person`,  the `update relationship` action works on that `Person`.

**The `implicit update relationship` action** 
the **Delete**  pipeline also allows for an `implicit update relationship` action on an already existing resource. Eg. when deleting an `Article` that was related to a `Person`,  the relationship between them is altered, and an `implicit update relationship` action is performed on that `Person`. This update is "implicit" in the sense that no explicit information about that `Person` was provided in the request that triggered this pipeline. See [update relationship vs implicit update relationship](www.will-add-link-later.com) for more information.

### Shared Actions
Note that **some actions are shared across pipelines**. For example, both the **Post** and **Patch** pipeline can perform the `update relationship`  action on an (already existing) involved resource. Similarly, the **Get** and **GetSingle** pipelines perform the same `read` action.
<br><br>
For a complete list of actions associated with each pipeline, see the [overview table below](#hook-execution-overview)

## Hooks
For all actions it is possible to implement **at least one hook** to intercept its execution. These hooks can be implemented by overriding the default virtual  implementation on `ResourceDefintion<T>`. Note that the default implementation is a dummy implementation, which is ignored when firing hooks.

### Hooks for repository actions
For example, the `create` action for a particular resource can be intercepted by overriding
* the `ResourceDefintion<Article>.BeforeCreate` hook for custom logic **just before** execution of the main `create` action
* the `ResourceDefintion<Article>.AfterCreate` hook for custom logic **just after** execution of the main `create` action

If with the creation of an `Article`  a relationship to `Person` is updated simultaneously, this can be intercepted by 
* the `ResourceDefintion<Person>.BeforeUpdateRelationship` hook for custom logic **just before** execution of the nested `update relationship` action.
* the `ResourceDefintion<Person>.AfterUpdateRelationship` hook for custom logic **just after** execution of the nested `update relationship` action.

### OnReturn hook
Another action that is shared across multiple pipelines is the `return` action. Although not strictly compatible with the *CRUD* vocabulary, and although not executed by the Repository Layer, pipelines are also said to perform a `return` action when any content is to be returned from the API. For example, the **Delete** pipeline does not return any content, but a *HTTP 204 No Content* instead, and will therefore not perform a `return` action. On the contrary, the **Get** pipeline does return content, and will therefore perform a `return action`

Any return content can be intercepted and transformed by implementing the `ResourceDefintion<TEntity>.OnReturn` hook which intercepts the `return` action. For this action, there is no distinction between a `Before` and `After` hook, because no code after a `return` statement can be evaluated. Note that the `return` action can work on *main resources as well as nested resources*, see  [this example below](www.will-add-link-later.com).
<br><br>
For an overview of all pipelines, hooks and actions, see the table below, and for more detailed information about the available hooks, see the [IResourceHookContainer<T>](www.will-add-link-later.com) interface.

# 2. Hook execution overview


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
    <td align="center">1. BeforeUpdate<br>2. BeforeUpdateRelationship<br>3. BeforeImplicitUpdateRelationship</td>
    <td align="center" colspan="2">update<br>update relationship<br>implicit update relationship</td>
    <td align="center">1. AfterUpdate<br>2. AfterUpdateRelationship</td>
    <td align="center">✅</td>
  </tr>
  <tr>
    <td>PatchRelationship</td>
    <td align="center">1. BeforeUpdate<br>2. BeforeUpdateRelationship</td>
    <td align="center" colspan="2">update<br>update relationship<br>implicit update relationship</td>
    <td align="center">1. AfterUpdate<br>2. AfterUpdateRelationship</td>
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


# 3. Examples: basic usage

## Most minimal example
The simplest example does not require much explanation. This hook would triggered by any default JsonApiDotNetCore API route for `Article`.
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
Now, let's assume our application has two resources: `Article` and `Person`, and that there exist a one-to-one  and one-to-many relationship between them (`Article has one Author` and `Article has many Reviewers`). Let's assume we need to log the following actions we want to have logs when an API user
* An API user deleting an article
* An API user removes the `Author` relationship of a person
* An API user removes the `Reviewer` relationship of a person

This could be achieved in the following way
```c#
/// Note that resource definitions are also injected as scoped services.
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
Using the `OnReturn` hook, any set of resources can be manipulated as desired before serving it from the API. One of the use-cases for this is having a workaround for the long awaited support of [filtered includes](https://github.com/aspnet/EntityFrameworkCore/issues/1833) within Entity Framework.

As an example, consider again an application with the `Article`  and `Person` resource, and let's assume the following business rules
* when reading `Article`s, we never want to show articles for which the `SoftDeleted` property is set to true.
* when reading `Person`s, we never want to show people who wish to remain anonymous (`WantsPrivacy` is set to true).

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
            return entities.Where(p => p.WantsPrivacy == false);
        }
        return entities;
    }
}
```
Note that not only privacy enabled people will be filtered when directly performing a `GET /people`, but also when included through relationships, like `GET /articles?include=author,reviewers`. Simultaneously, `if` condition that checks for `ResourcePipeline.Get` in the `PersonResource` ensures we still get expected responses from the API when eg. creating a person with `WantsPrivacy` set to true.

# 3. Examples: advanced usage

## Simple authorization: explicitly affected resources
Resource hooks can be used to easily implement authorization in your application.  As an example, consider the case in which an API user is not allowed to see "secret" people, which is reflected by the `IsSecret` property on `Person`  being set to true`true`.  The API should such scenarios as follows:
* When reading people (`GET /people`), it should hide all people that are set to secret.
* When reading a single person (`GET /people/{id}`), throw an authorization error the particular requested person is set to secret.

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
This example of authorizatin is considered simple because it only involves one resource. The next example shows a more complex case

## Advanced authorization: implicitly affected resources
Let's consider an authorization scenario for which we are required to implement multiple hooks across multiple resource definitions. We will assume the following:
* There exists a one-to-one relationship between `Article` and `Person`: an article can have only one author, and a person can be author of at most one article.
* The author of article `Old Article` is person `Alice`.
* The author of article `New Article` is person `Bob`.

Now let's consider a API user that tries to update `New Article` by setting its author to `Alice`. First to all, we wish to authorize this operation by the verifying permissions related to the resources that are **explicity affected**  by it:
1. Is the API user allowed to update `New Article`?
2. Is the API user allowed to update `Alice`?

Apart from this, we also wish to verify permissions for the resources that are **implicitly affected** by this operation: `Bob` and `Old Article`. Note that setting `Alice` as the new author of `New Article` will result in removing the following two relationships:  `Bob` being an author of `New Article`, and `Alice` being an author of  `Old Article`. Therefore, we wish wish to verify the related permissions:
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
If your application is built using a microservices infrastructure, it may be relevant to propagate data changes between microservices, [see this article for more information](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/integration-event-based-microservice-communications). In this example, we will assume the implementation of an event bus and show to generate data consistency integration events using resource hooks.

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
            var @event = new ResourceUpdatedEvent(article, properties: _context.AttributesToUpdate);
            _bus.Publish(@event);
        }
    }
}
```

## Hooks for many-to-many join tables
In this example we consider an application with a many-to-many relationships: `Article` and `Tag`, with an internally used `ArticleTag` throughtype.

Usually, join table records will not contain any extra information other than that which is used internally for the many-to-many relationship. The class declaration will then look like this:
```c#
public class ArticleTag
{
    public int ArticleId { get; set; }
    public Article Article { get; set; }
    public int TagId { get; set; }
    public Tag Tag { get; set; }
}
```
In this case, there shouldn't be any situations in which it is useful to be able to implement hooks for `ArticleTag`. If we then eg. implement the `AfterRead` and `OnReturn` hook for `Article` and `Tag`, and perform a `GET /articles?include=tags` request, we may expect the following order of execution:

1. Article AfterRead
2. Tag AfterRead 
3. Article OnReturn
4. Tag OnReturn

Sometimes, however, relevant data may be stored in the join table of a many-to-many relationship.  For example, we will want to add a property `LinkDate` to the join table that reflects when a tag was added to an article. In this case, we may want to execute business logic related to these records: we may want to hide any tags that were added to an article longer than 2 weeks ago.

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
