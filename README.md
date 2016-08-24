# JSON API .Net Core

JSON API Spec Conformance: **Non Conforming**

## Usage

- Configure the service:

```
services.AddDbContext<ApplicationDbContext>(options =>
  options.UseNpgsql(Configuration["Data:ConnectionString"]),
  ServiceLifetime.Transient);

services.AddJsonApi(config => {
  config.UseContext<ApplicationDbContext>();
  config.SetDefaultNamespace("api/v1");
});
```

- Add middleware:

```
app.UseJsonApi();
```

## Example Requests

### GET TodoItems

Request:

```
curl -X GET 
  -H "Content-Type: application/vnd.api+json" 
  "http://localhost:5000/api/v1/todoItems/"
```

Response:

```
{
  "links": {
    "self": "http://localhost:5000/api/v1/todoItems/"
  },
  "data": [
    {
      "type": "todoItems",
      "id": "2",
      "attributes": {
        "name": "Something To Do"
      },
      "relationships": {
        "owner": {
          "self": "http://localhost:5000/api/v1/todoItems/2/relationships/owner",
          "related": "http://localhost:5000/api/v1/todoItems/2/owner"
        }
      },
      "links": {
        "self": "http://localhost:5000/api/v1/todoItems/2"
      }
    }
  ]
}
```

### Get People/{id}
Request:

```
curl -X GET 
  -H "Content-Type: application/vnd.api+json" 
  "http://localhost:5000/api/v1/people/1"
```

Response:

```
{
  "links": {
    "self": "http://localhost:5000/api/v1/people/1"
  },
  "data": {
    "type": "people",
    "id": "1",
    "attributes": {
      "name": "Captain Obvious"
    },
    "relationships": {
      "todoItems": {
        "self": "http://localhost:5000/api/v1/people/1/relationships/todoItems",
        "related": "http://localhost:5000/api/v1/people/1/todoItems"
      }
    },
    "links": {
      "self": "http://localhost:5000/api/v1/people/1"
    }
  }
}
```

## References
[JsonApi Specification](http://jsonapi.org/)

## Current Assumptions

- Using Entity Framework
- All entities in the specified context should have controllers
- All entities are served from the same namespace (i.e. 'api/v1')
