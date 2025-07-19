# State Transitions in Resource Updates

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/InputValidation/RequestBody) shows how to validate state transitions when updating a resource.

This feature is implemented using a custom resource definition:

- The `Workflow` resource type contains a `Stage` property of type `WorkflowStage`.
- The `WorkflowStage` enumeration lists a workflow's possible states.
- `WorkflowDefinition` contains a hard-coded stage transition table defining the valid transitions. For example, a workflow in stage `InProgress` can be changed to `OnHold` or `Canceled`, but not `Created`.
  - The `OnPrepareWriteAsync` method is overridden to capture the stage currently stored in the database in the `_previousStage` private field.
  - The `OnWritingAsync` method is overridden to verify whether the stage change is permitted. It consults the stage transition table to determine whether there's a path from `_previousStage` to the to-be-stored stage, producing an error if there isn't.
