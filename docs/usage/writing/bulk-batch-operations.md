# Bulk/batch

_since v4.1_

The [Atomic Operations](https://jsonapi.org/ext/atomic/) JSON:API extension defines
how to perform multiple write operations in a linear and atomic manner.

Clients can send an array of operations in a single request. JsonApiDotNetCore guarantees that those
operations will be processed in order and will either completely succeed or fail together.

On failure, the zero-based index of the failing operation is returned in the `error.source.pointer` field of the error response.

## Usage

To enable operations, add a controller to your project that inherits from `JsonApiOperationsController` or `BaseJsonApiOperationsController`:

```c#
public sealed class OperationsController : JsonApiOperationsController
{
    public OperationsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
        IOperationsProcessor processor, IJsonApiRequest request,
        ITargetedFields targetedFields)
        : base(options, loggerFactory, processor, request, targetedFields)
    {
    }
}
```

You'll need to send the next Content-Type in a POST request for operations:

```
application/vnd.api+json; ext="https://jsonapi.org/ext/atomic"
```

### Local IDs

Local IDs (lid) can be used to associate resources that have not yet been assigned an ID.
The next example creates two resources and sets a relationship between them:

```json
POST http://localhost/api/operations HTTP/1.1
Content-Type: application/vnd.api+json;ext="https://jsonapi.org/ext/atomic"

{
  "atomic:operations": [
    {
      "op": "add",
      "data": {
        "type": "musicTracks",
        "lid": "id-for-i-will-survive",
        "attributes": {
          "title": "I will survive"
        }
      }
    },
    {
      "op": "add",
      "data": {
        "type": "performers",
        "lid": "id-for-gloria-gaynor",
        "attributes": {
          "artistName": "Gloria Gaynor"
        }
      }
    },
    {
      "op": "update",
      "ref": {
        "type": "musicTracks",
        "lid": "id-for-i-will-survive",
        "relationship": "performers"
      },
      "data": [
        {
          "type": "performers",
          "lid": "id-for-gloria-gaynor"
        }
      ]
    }
  ]
}
```

For example requests, see our suite of tests in JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.

## Configuration

The maximum number of operations per request defaults to 10, which you can change from Startup.cs:

```c#
services.AddJsonApi(options => options.MaximumOperationsPerRequest = 250);
```

Or, if you want to allow unconstrained, set it to `null` instead.

### Multiple controllers

You can register multiple operations controllers using custom routes, for example:

```c#
[DisableRoutingConvention, Route("/operations/musicTracks/create")]
public sealed class CreateMusicTrackOperationsController : JsonApiOperationsController
{
    public override async Task<IActionResult> PostOperationsAsync(
        IList<OperationContainer> operations, CancellationToken cancellationToken)
    {
        AssertOnlyCreatingMusicTracks(operations);

        return await base.PostOperationsAsync(operations, cancellationToken);
    }
}
```

## Limitations

For our atomic:operations implementation, the next limitations apply:

- The `ref.href` field cannot be used. Use type/id or type/lid instead.
- You cannot both assign and reference the same local ID in a single operation.
- All repositories used in an operations request must implement `IRepositorySupportsTransaction` and participate in the same transaction.
- If you're not using Entity Framework Core, you'll need to implement and register `IOperationsTransactionFactory` yourself.
