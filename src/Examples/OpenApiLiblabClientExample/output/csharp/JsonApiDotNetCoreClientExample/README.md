# JsonApiDotNetCoreClientExample C# SDK 1.0.0

A C# SDK for JsonApiDotNetCoreClientExample.

- API version: 1.0
- SDK version: 1.0.0

## Table of Contents

- [Authentication](#authentication)
- [Services](#services)

## Authentication

### Access Token

The JsonApiDotNetCoreClientExample API uses a access token as a form of authentication.

The access token can be set when initializing the SDK like this:

```cs
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Config;

var config = new JsonApiDotNetCoreClientExampleConfig()
{
	AccessToken = "YOUR_ACCESS_TOKEN"
};

var client = new JsonApiDotNetCoreClientExampleClient(config);
```

Or at a later stage:

```cs
client.SetAccessToken("YOUR_ACCESS_TOKEN")
```

## Services

### PeopleService

#### **GetPersonCollectionAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.People.GetPersonCollectionAsync(new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PostPersonAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var data = new PersonDataInPostRequest(PersonResourceType.People);
var input = new PersonPostRequestDocument(data);

var response = await client.People.PostPersonAsync(input, new object());

Console.WriteLine(response);
```

#### **HeadPersonCollectionAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.People.HeadPersonCollectionAsync(new object(), "If-None-Match");
```

#### **GetPersonAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.People.GetPersonAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PatchPersonAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var data = new PersonDataInPatchRequest(PersonResourceType.People, "amet de");
var input = new PersonPatchRequestDocument(data);

var response = await client.People.PatchPersonAsync(input, "id", new object());

Console.WriteLine(response);
```

#### **DeletePersonAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.People.DeletePersonAsync("id");
```

#### **HeadPersonAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.People.HeadPersonAsync("id", new object(), "If-None-Match");
```

#### **GetPersonAssignedTodoItemsAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.People.GetPersonAssignedTodoItemsAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **HeadPersonAssignedTodoItemsAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.People.HeadPersonAssignedTodoItemsAsync("id", new object(), "If-None-Match");
```

#### **GetPersonAssignedTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.People.GetPersonAssignedTodoItemsRelationshipAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PostPersonAssignedTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TodoItemIdentifier(TodoItemResourceType.TodoItems, "consequat cil");
var data = new List<TodoItemIdentifier>() { dataItem };
var input = new ToManyTodoItemInRequest(data);

await client.People.PostPersonAssignedTodoItemsRelationshipAsync(input, "id");
```

#### **PatchPersonAssignedTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TodoItemIdentifier(TodoItemResourceType.TodoItems, "consequat cil");
var data = new List<TodoItemIdentifier>() { dataItem };
var input = new ToManyTodoItemInRequest(data);

await client.People.PatchPersonAssignedTodoItemsRelationshipAsync(input, "id");
```

#### **DeletePersonAssignedTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TodoItemIdentifier(TodoItemResourceType.TodoItems, "consequat cil");
var data = new List<TodoItemIdentifier>() { dataItem };
var input = new ToManyTodoItemInRequest(data);

await client.People.DeletePersonAssignedTodoItemsRelationshipAsync(input, "id");
```

#### **HeadPersonAssignedTodoItemsRelationshipAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.People.HeadPersonAssignedTodoItemsRelationshipAsync("id", new object(), "If-None-Match");
```

#### **GetPersonOwnedTodoItemsAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.People.GetPersonOwnedTodoItemsAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **HeadPersonOwnedTodoItemsAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.People.HeadPersonOwnedTodoItemsAsync("id", new object(), "If-None-Match");
```

#### **GetPersonOwnedTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.People.GetPersonOwnedTodoItemsRelationshipAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PostPersonOwnedTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TodoItemIdentifier(TodoItemResourceType.TodoItems, "consequat cil");
var data = new List<TodoItemIdentifier>() { dataItem };
var input = new ToManyTodoItemInRequest(data);

await client.People.PostPersonOwnedTodoItemsRelationshipAsync(input, "id");
```

#### **PatchPersonOwnedTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TodoItemIdentifier(TodoItemResourceType.TodoItems, "consequat cil");
var data = new List<TodoItemIdentifier>() { dataItem };
var input = new ToManyTodoItemInRequest(data);

await client.People.PatchPersonOwnedTodoItemsRelationshipAsync(input, "id");
```

#### **DeletePersonOwnedTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TodoItemIdentifier(TodoItemResourceType.TodoItems, "consequat cil");
var data = new List<TodoItemIdentifier>() { dataItem };
var input = new ToManyTodoItemInRequest(data);

await client.People.DeletePersonOwnedTodoItemsRelationshipAsync(input, "id");
```

#### **HeadPersonOwnedTodoItemsRelationshipAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.People.HeadPersonOwnedTodoItemsRelationshipAsync("id", new object(), "If-None-Match");
```

### TagsService

#### **GetTagCollectionAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.Tags.GetTagCollectionAsync(new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PostTagAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var data = new TagDataInPostRequest(TagResourceType.Tags);
var input = new TagPostRequestDocument(data);

var response = await client.Tags.PostTagAsync(input, new object());

Console.WriteLine(response);
```

#### **HeadTagCollectionAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.Tags.HeadTagCollectionAsync(new object(), "If-None-Match");
```

#### **GetTagAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.Tags.GetTagAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PatchTagAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var data = new TagDataInPatchRequest(TagResourceType.Tags, "pro");
var input = new TagPatchRequestDocument(data);

var response = await client.Tags.PatchTagAsync(input, "id", new object());

Console.WriteLine(response);
```

#### **DeleteTagAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.Tags.DeleteTagAsync("id");
```

#### **HeadTagAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.Tags.HeadTagAsync("id", new object(), "If-None-Match");
```

#### **GetTagTodoItemsAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.Tags.GetTagTodoItemsAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **HeadTagTodoItemsAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.Tags.HeadTagTodoItemsAsync("id", new object(), "If-None-Match");
```

#### **GetTagTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.Tags.GetTagTodoItemsRelationshipAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PostTagTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TodoItemIdentifier(TodoItemResourceType.TodoItems, "consequat cil");
var data = new List<TodoItemIdentifier>() { dataItem };
var input = new ToManyTodoItemInRequest(data);

await client.Tags.PostTagTodoItemsRelationshipAsync(input, "id");
```

#### **PatchTagTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TodoItemIdentifier(TodoItemResourceType.TodoItems, "consequat cil");
var data = new List<TodoItemIdentifier>() { dataItem };
var input = new ToManyTodoItemInRequest(data);

await client.Tags.PatchTagTodoItemsRelationshipAsync(input, "id");
```

#### **DeleteTagTodoItemsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TodoItemIdentifier(TodoItemResourceType.TodoItems, "consequat cil");
var data = new List<TodoItemIdentifier>() { dataItem };
var input = new ToManyTodoItemInRequest(data);

await client.Tags.DeleteTagTodoItemsRelationshipAsync(input, "id");
```

#### **HeadTagTodoItemsRelationshipAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.Tags.HeadTagTodoItemsRelationshipAsync("id", new object(), "If-None-Match");
```

### TodoItemsService

#### **GetTodoItemCollectionAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.TodoItems.GetTodoItemCollectionAsync(new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PostTodoItemAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var data = new TodoItemDataInPostRequest(TodoItemResourceType.TodoItems);
var input = new TodoItemPostRequestDocument(data);

var response = await client.TodoItems.PostTodoItemAsync(input, new object());

Console.WriteLine(response);
```

#### **HeadTodoItemCollectionAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.TodoItems.HeadTodoItemCollectionAsync(new object(), "If-None-Match");
```

#### **GetTodoItemAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.TodoItems.GetTodoItemAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PatchTodoItemAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var data = new TodoItemDataInPatchRequest(TodoItemResourceType.TodoItems, "aliqua do");
var input = new TodoItemPatchRequestDocument(data);

var response = await client.TodoItems.PatchTodoItemAsync(input, "id", new object());

Console.WriteLine(response);
```

#### **DeleteTodoItemAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.TodoItems.DeleteTodoItemAsync("id");
```

#### **HeadTodoItemAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.TodoItems.HeadTodoItemAsync("id", new object(), "If-None-Match");
```

#### **GetTodoItemAssigneeAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.TodoItems.GetTodoItemAssigneeAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **HeadTodoItemAssigneeAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.TodoItems.HeadTodoItemAssigneeAsync("id", new object(), "If-None-Match");
```

#### **GetTodoItemAssigneeRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.TodoItems.GetTodoItemAssigneeRelationshipAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PatchTodoItemAssigneeRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var data = new PersonIdentifier(PersonResourceType.People, "proident et");
var input = new NullableToOnePersonInRequest(data);

await client.TodoItems.PatchTodoItemAssigneeRelationshipAsync(input, "id");
```

#### **HeadTodoItemAssigneeRelationshipAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.TodoItems.HeadTodoItemAssigneeRelationshipAsync("id", new object(), "If-None-Match");
```

#### **GetTodoItemOwnerAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.TodoItems.GetTodoItemOwnerAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **HeadTodoItemOwnerAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.TodoItems.HeadTodoItemOwnerAsync("id", new object(), "If-None-Match");
```

#### **GetTodoItemOwnerRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.TodoItems.GetTodoItemOwnerRelationshipAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PatchTodoItemOwnerRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var data = new PersonIdentifier(PersonResourceType.People, "proident et");
var input = new ToOnePersonInRequest(data);

await client.TodoItems.PatchTodoItemOwnerRelationshipAsync(input, "id");
```

#### **HeadTodoItemOwnerRelationshipAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.TodoItems.HeadTodoItemOwnerRelationshipAsync("id", new object(), "If-None-Match");
```

#### **GetTodoItemTagsAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.TodoItems.GetTodoItemTagsAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **HeadTodoItemTagsAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.TodoItems.HeadTodoItemTagsAsync("id", new object(), "If-None-Match");
```

#### **GetTodoItemTagsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

var response = await client.TodoItems.GetTodoItemTagsRelationshipAsync("id", new object(), "If-None-Match");

Console.WriteLine(response);
```

#### **PostTodoItemTagsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TagIdentifier(TagResourceType.Tags, "ip");
var data = new List<TagIdentifier>() { dataItem };
var input = new ToManyTagInRequest(data);

await client.TodoItems.PostTodoItemTagsRelationshipAsync(input, "id");
```

#### **PatchTodoItemTagsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TagIdentifier(TagResourceType.Tags, "ip");
var data = new List<TagIdentifier>() { dataItem };
var input = new ToManyTagInRequest(data);

await client.TodoItems.PatchTodoItemTagsRelationshipAsync(input, "id");
```

#### **DeleteTodoItemTagsRelationshipAsync**

```csharp
using JsonApiDotNetCoreClientExample;
using JsonApiDotNetCoreClientExample.Models;

var client = new JsonApiDotNetCoreClientExampleClient();

var dataItem = new TagIdentifier(TagResourceType.Tags, "ip");
var data = new List<TagIdentifier>() { dataItem };
var input = new ToManyTagInRequest(data);

await client.TodoItems.DeleteTodoItemTagsRelationshipAsync(input, "id");
```

#### **HeadTodoItemTagsRelationshipAsync**

Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.

```csharp
using JsonApiDotNetCoreClientExample;

var client = new JsonApiDotNetCoreClientExampleClient();

await client.TodoItems.HeadTodoItemTagsRelationshipAsync("id", new object(), "If-None-Match");
```

<!-- This file was generated by liblab | https://liblab.com/ -->
