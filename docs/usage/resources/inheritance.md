# Resource inheritance

_since v5.0_

Resource classes can be part of a type hierarchy. For example:

```c#
#nullable enable

public abstract class Human : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasOne]
    public Man? Father { get; set; }

    [HasOne]
    public Woman? Mother { get; set; }

    [HasMany]
    public ISet<Human> Children { get; set; } = new HashSet<Human>();

    [HasOne]
    public Human? BestFriend { get; set; }
}

public sealed class Man : Human
{
    [Attr]
    public bool HasBeard { get; set; }

    [HasOne]
    public Woman? Wife { get; set; }
}

public sealed class Woman : Human
{
    [Attr]
    public string? MaidenName { get; set; }

    [HasOne]
    public Man? Husband { get; set; }
}
```

## Reading data

You can access them through base or derived endpoints.

```http
GET /humans HTTP/1.1

{
  "data": [
    {
      "type": "women",
      "id": "1",
      "attributes": {
        "maidenName": "Smith",
        "name": "Jane Doe"
      },
      "relationships": {
        "husband": {
          "links": {
            "self": "/women/1/relationships/husband",
            "related": "/women/1/husband"
          }
        },
        "father": {
          "links": {
            "self": "/women/1/relationships/father",
            "related": "/women/1/father"
          }
        },
        "mother": {
          "links": {
            "self": "/women/1/relationships/mother",
            "related": "/women/1/mother"
          }
        },
        "children": {
          "links": {
            "self": "/women/1/relationships/children",
            "related": "/women/1/children"
          }
        },
        "bestFriend": {
          "links": {
            "self": "/women/1/relationships/bestFriend",
            "related": "/women/1/bestFriend"
          }
        }
      },
      "links": {
        "self": "/women/1"
      }
    },
    {
      "type": "men",
      "id": "2",
      "attributes": {
        "hasBeard": true,
        "name": "John Doe"
      },
      "relationships": {
        "wife": {
          "links": {
            "self": "/men/2/relationships/wife",
            "related": "/men/2/wife"
          }
        },
        "father": {
          "links": {
            "self": "/men/2/relationships/father",
            "related": "/men/2/father"
          }
        },
        "mother": {
          "links": {
            "self": "/men/2/relationships/mother",
            "related": "/men/2/mother"
          }
        },
        "children": {
          "links": {
            "self": "/men/2/relationships/children",
            "related": "/men/2/children"
          }
        },
        "bestFriend": {
          "links": {
            "self": "/men/2/relationships/bestFriend",
            "related": "/men/2/bestFriend"
          }
        }
      },
      "links": {
        "self": "/men/2"
      }
    }
  ]
}
```

### Spare fieldsets

If you only want to retrieve the fields from the base type, you can use [sparse fieldsets](~/usage/reading/sparse-fieldset-selection.md).

```http
GET /humans?fields[men]=name,children&fields[women]=name,children HTTP/1.1
```

### Includes

Relationships on derived types can be included without special syntax.

```http
GET /humans?include=husband,wife,children HTTP/1.1
```

### Sorting

Just like includes, you can sort on derived attributes and relationships.

```http
GET /humans?sort=maidenName,wife.name HTTP/1.1
```

This returns all women sorted by their maiden names, followed by all men sorted by the name of their wife.

To accomplish the same from a [Resource Definition](~/usage/extensibility/resource-definitions.md), upcast to the derived type:

```c#
public override SortExpression OnApplySort(SortExpression? existingSort)
{
    return CreateSortExpressionFromLambda(new PropertySortOrder
    {
        (human => ((Woman)human).MaidenName, ListSortDirection.Ascending),
        (human => ((Man)human).Wife!.Name, ListSortDirection.Ascending)
    });
}
```

### Filtering

Use the `isType` filter function to perform a type check on a derived type. You can pass a nested filter, where the derived fields are accessible.

Only return men:
```http
GET /humans?filter=isType(,men) HTTP/1.1
```

Only return men with beards:
```http
GET /humans?filter=isType(,men,equals(hasBeard,'true')) HTTP/1.1
```

The first parameter of `isType` can be used to perform the type check on a to-one relationship path.

Only return people whose best friend is a man with children:
```http
GET /humans?filter=isType(bestFriend,men,has(children)) HTTP/1.1
```

Only return people who have at least one female married child:
```http
GET /humans?filter=has(children,isType(,woman,not(equals(husband,null)))) HTTP/1.1
```

## Writing data

Just like reading data, you can use base or derived endpoints. When using relationships in request bodies, you can use base or derived types as well.
The only exception is that you cannot use an abstract base type in the request body when creating or updating a resource.

For example, updating an attribute and relationship can be done at an abstract endpoint, but its body requires non-abstract types:

```http
PATCH /humans/2 HTTP/1.1

{
  "data": {
    "type": "men",
    "id": "2",
    "attributes": {
      "hasBeard": false
    },
    "relationships": {
      "wife": {
        "data": {
          "type": "women",
          "id": "1"
        }
      }
    }
  }
}
```

Updating a relationship does allow abstract types. For example:

```http
PATCH /humans/1/relationships/children HTTP/1.1

{
  "data": [
    {
      "type": "humans",
      "id": "2"
    }
  ]
}
```

### Request pipeline

The `TResource` type parameter used in controllers, resource services and resource repositories always matches the used endpoint.
But when JsonApiDotNetCore sees usage of a type from a type hierarchy, it fetches the stored types and updates `IJsonApiRequest` accordingly.
As a result, `TResource` can be different from what `IJsonApiRequest.PrimaryResourceType` returns.

For example, on the request:
```http
 GET /humans/1 HTTP/1.1
```

JsonApiDotNetCore runs `IResourceService<Human, long>`, but `IJsonApiRequest.PrimaryResourceType` returns `Woman`
if human with ID 1 is stored as a woman in the underlying data store.

Even with a simple type hierarchy as used here, lots of possible combinations quickly arise. For example, changing someone's best friend can be done using the following requests:
- `PATCH /humans/1/ { "data": { relationships: { bestFriend: { type: "women" ... } } } }`
- `PATCH /humans/1/ { "data": { relationships: { bestFriend: { type: "men" ... } } } }`
- `PATCH /women/1/ { "data": { relationships: { bestFriend: { type: "women" ... } } } }`
- `PATCH /women/1/ { "data": { relationships: { bestFriend: { type: "men" ... } } } }`
- `PATCH /men/2/ { "data": { relationships: { bestFriend: { type: "women" ... } } } }`
- `PATCH /men/2/ { "data": { relationships: { bestFriend: { type: "men" ... } } } }`
- `PATCH /humans/1/relationships/bestFriend { "data": { type: "human" ... } }`
- `PATCH /humans/1/relationships/bestFriend { "data": { type: "women" ... } }`
- `PATCH /humans/1/relationships/bestFriend { "data": { type: "men" ... } }`
- `PATCH /women/1/relationships/bestFriend { "data": { type: "human" ... } }`
- `PATCH /women/1/relationships/bestFriend { "data": { type: "women" ... } }`
- `PATCH /women/1/relationships/bestFriend { "data": { type: "men" ... } }`
- `PATCH /men/2/relationships/bestFriend { "data": { type: "human" ... } }`
- `PATCH /men/2/relationships/bestFriend { "data": { type: "women" ... } }`
- `PATCH /men/2/relationships/bestFriend { "data": { type: "men" ... } }`

Because of all the possible combinations, implementing business rules in the pipeline is a no-go.
Resource definitions provide a better solution, see below.

### Resource definitions

In contrast to the request pipeline, JsonApiDotNetCore always executes the resource definition that matches the *stored* type.
This enables to implement business logic in a central place, irrespective of which endpoint was used or whether base types were used in relationships.

To delegate logic for base types to their matching resource type, you can build a chain of resource definitions. And because you'll always get the
actually stored types (for relationships too), you can type-check left-side and right-side types in resources definitions.

```c#
public sealed class HumanDefinition : JsonApiResourceDefinition<Human, long>
{
    public HumanDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override Task<IIdentifiable?> OnSetToOneRelationshipAsync(Human leftResource,
        HasOneAttribute hasOneRelationship, IIdentifiable? rightResourceId,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (leftResource is Man &&
            hasOneRelationship.Property.Name == nameof(Human.BestFriend) &&
            rightResourceId is Woman)
        {
            throw new Exception("Men are not supposed to have a female best friend.");
        }

        return Task.FromResult(rightResourceId);
    }

    public override Task OnWritingAsync(Human resource, WriteOperationKind writeOperation,
        CancellationToken cancellationToken)
    {
        if (writeOperation is WriteOperationKind.CreateResource or
            WriteOperationKind.UpdateResource)
        {
            if (resource is Man { HasBeard: true })
            {
                throw new Exception("Only shaved men, please.");
            }
        }

        return Task.CompletedTask;
    }
}

public sealed class WomanDefinition : JsonApiResourceDefinition<Woman, long>
{
    private readonly IResourceDefinition<Human, long> _baseDefinition;

    public WomanDefinition(IResourceGraph resourceGraph,
        IResourceDefinition<Human, long> baseDefinition)
        : base(resourceGraph)
    {
        _baseDefinition = baseDefinition;
    }

    public override Task<IIdentifiable?> OnSetToOneRelationshipAsync(Woman leftResource,
        HasOneAttribute hasOneRelationship, IIdentifiable? rightResourceId,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (ResourceType.BaseType!.FindRelationshipByPublicName(
            hasOneRelationship.PublicName) != null)
        {
            // Delegate to resource definition for base type Human.
            return _baseDefinition.OnSetToOneRelationshipAsync(leftResource,
                hasOneRelationship, rightResourceId, writeOperation, cancellationToken);
        }

        // Handle here.
        if (hasOneRelationship.Property.Name == nameof(Woman.Husband) &&
            rightResourceId == null)
        {
            throw new Exception("We don't accept unmarried women at this time.");
        }

        return Task.FromResult(rightResourceId);
    }

    public override async Task OnPrepareWriteAsync(Woman resource,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        // Run rules in resource definition for base type Human.
        await _baseDefinition.OnPrepareWriteAsync(resource, writeOperation, cancellationToken);

        // Run rules for type Woman.
        if (resource.MaidenName == null)
        {
            throw new Exception("Women should have a maiden name.");
        }
    }
}

public sealed class ManDefinition : JsonApiResourceDefinition<Man, long>
{
    private readonly IResourceDefinition<Human, long> _baseDefinition;

    public ManDefinition(IResourceGraph resourceGraph,
        IResourceDefinition<Human, long> baseDefinition)
        : base(resourceGraph)
    {
        _baseDefinition = baseDefinition;
    }

    public override Task<IIdentifiable?> OnSetToOneRelationshipAsync(Man leftResource,
        HasOneAttribute hasOneRelationship, IIdentifiable? rightResourceId,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        // No man-specific logic, but we'll still need to delegate.
        return _baseDefinition.OnSetToOneRelationshipAsync(leftResource, hasOneRelationship,
            rightResourceId, writeOperation, cancellationToken);
    }

    public override Task OnWritingAsync(Man resource, WriteOperationKind writeOperation,
        CancellationToken cancellationToken)
    {
        // No man-specific logic, but we'll still need to delegate.
        return _baseDefinition.OnWritingAsync(resource, writeOperation, cancellationToken);
    }
}
```
