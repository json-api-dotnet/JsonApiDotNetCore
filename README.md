# JSON API .Net Core

## Usage

- Confiure the service:

```
services.AddJsonApiDotNetCore(config => {
  config.UseContext<ApplicationDbContext>();
  config.SetDefaultNamespace("api/v1");

  // add json api models
  config.AddModel<TodoItem>();
  config.AddModel<TodoItemCollection>();
});
```

- Add middleware:

```
app.UseJsonApi();
```
