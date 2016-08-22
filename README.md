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


## TODO

- Middleware should check whether or not the route has been configured and create the controller instance
  - [ ] GET /{namespace}/{entities}/
  - [ ] GET /{namespace}/{entities}/{id}
  - [ ] POST /{namespace}/{entities}/
  - [ ] PUT /{namespace}/{entities}/{id}
  - [ ] DELETE /{namespace}/{entities}/{id}
  - [ ] PATCH /{namespace}/{entities}/{id}
- [ ] Check to see if there is a controller override specified (interface vs. abstract vs. concrete controller), if so call the override methods instead
- [ ] End the request pipeline

