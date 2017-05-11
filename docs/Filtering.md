---
currentMenu: filtering
---

# Filtering

You can filter resources by attributes using the `filter` query parameter. 
By default, all attributes are filterable.
The filtering strategy we have selected, uses the following form:

```
?filter[attribute]=value
```

For operations other than equality, the query can be prefixed with an operation
identifier):

```
?filter[attribute]=eq:value
?filter[attribute]=lt:value
?filter[attribute]=gt:value
?filter[attribute]=le:value
?filter[attribute]=ge:value
?filter[attribute]=like:value
```

### Custom Filters

You can customize the filter implementation by overriding the method in the `DefaultEntityRepository` like so:

```csharp
public class MyEntityRepository : DefaultEntityRepository<MyEntity>
{
    public MyEntityRepository(
    	AppDbContext context,
        ILoggerFactory loggerFactory,
        IJsonApiContext jsonApiContext)
    : base(context, loggerFactory, jsonApiContext)
    { }
    
    public override IQueryable<TEntity> Filter(IQueryable<TEntity> entities,  FilterQuery filterQuery)
    {
        // use the base filtering method    
        entities = base.Filter(entities, filterQuery);
	
        // implement custom method
        return ApplyMyCustomFilter(entities, filterQuery);
    }
}
```
