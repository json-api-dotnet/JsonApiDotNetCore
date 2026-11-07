# Filtering

_since v4.0_

Resources can be filtered by attributes using the `filter` query string parameter.
By default, all attributes are filterable.
The filtering strategy we have selected, uses the following form.

```
?filter=expression
```

Expressions are composed using the following functions:

| Operation                     | Function           | Example                                               |
|-------------------------------|--------------------|-------------------------------------------------------|
| Equality                      | `equals`           | `?filter=equals(lastName,'Smith')`                    |
| Less than                     | `lessThan`         | `?filter=lessThan(age,'25')`                          |
| Less than or equal to         | `lessOrEqual`      | `?filter=lessOrEqual(lastModified,'2001-01-01')`      |
| Greater than                  | `greaterThan`      | `?filter=greaterThan(duration,'6:12:14')`             |
| Greater than or equal to      | `greaterOrEqual`   | `?filter=greaterOrEqual(percentage,'33.33')`          |
| Contains text                 | `contains`         | `?filter=contains(description,'cooking')`             |
| Starts with text              | `startsWith`       | `?filter=startsWith(description,'The')`               |
| Ends with text                | `endsWith`         | `?filter=endsWith(description,'End')`                 |
| Equals one value from set     | `any`              | `?filter=any(chapter,'Intro','Summary','Conclusion')` |
| Collection contains items     | `has`              | `?filter=has(articles)`                               |
| Number of items in collection | `count`            | `?filter=greaterThan(count(articles),'1')`            |
| Type-check derived type (v5)  | `isType`           | `?filter=isType(,men)`                                |
| Negation                      | `not`              | `?filter=not(equals(lastName,null))`                  |
| Conditional logical OR        | `or`               | `?filter=or(has(orders),has(invoices))`               |
| Conditional logical AND       | `and`              | `?filter=and(has(orders),has(invoices))`              |

Comparison operators compare an attribute against a constant value (between quotes), null or another attribute.
The ordered comparisons (`lessThan`, `lessOrEqual`, `greaterThan`, `greaterOrEqual`) also work on text and enum attributes,
for example `?filter=greaterThan(lastName,'Smith')` and `?filter=lessThan(dayOfWeek,'Tuesday')`.
The ordering is determined by the database (its collation for text, the stored value for enums).

```http
GET /users?filter=equals(displayName,'Brian O''Connor') HTTP/1.1
```

```http
GET /users?filter=equals(displayName,null) HTTP/1.1
```

```http
GET /users?filter=equals(displayName,lastName) HTTP/1.1
```

Comparison operators can be combined with the `count` function, which acts on to-many relationships and (since v6.0) on
[collection attributes](#compound-attributes-and-collections):

```http
GET /blogs?filter=lessThan(count(owner.articles),'10') HTTP/1.1
```

```http
GET /customers?filter=greaterThan(count(orders),count(invoices)) HTTP/1.1
```

When filters are used multiple times on the same resource, they are combined using an OR operator.
The next request returns all customers that have orders -or- whose last name is Smith.

```http
GET /customers?filter=has(orders)&filter=equals(lastName,'Smith') HTTP/1.1
```

Aside from filtering on the resource being requested (which would be blogs in /blogs and articles in /blogs/1/articles),
filtering on to-many relationships can be done using bracket notation:

```http
GET /articles?include=author,tags&filter=equals(author.lastName,'Smith')&filter[tags]=any(label,'tech','design') HTTP/1.1
```

In the above request, the first filter is applied on the collection of articles, while the second one is applied on the nested collection of tags.

> [!WARNING]
> The request above does **not** hide articles without any matching tags! Use the `has` function with a filter condition (see below) to accomplish that.

Putting it all together, you can build quite complex filters, such as:

```http
GET /blogs?include=owner.articles.revisions&filter=and(or(equals(title,'Technology'),has(owner.articles)),not(equals(owner.lastName,null)))&filter[owner.articles]=equals(caption,'Two')&filter[owner.articles.revisions]=greaterThan(publishTime,'2005-05-05') HTTP/1.1
```

_since v4.2_

The `has` function takes an optional filter condition as second parameter, for example:

```http
GET /customers?filter=has(orders,not(equals(status,'Paid'))) HTTP/1.1
```

Which returns only customers that have at least one unpaid order.

## Compound attributes and collections

_since v6.0_

Field chains can go through compound attributes (using dots) and through to-one relationships, in any combination.
The examples below use this resource:

```c#
#nullable enable

public sealed class Contact : Identifiable<long>
{
    [Attr(IsCompound = true)]
    public PersonName? Name { get; set; }

    [Attr(IsCompound = true)]
    public List<PhoneNumber> PhoneNumbers { get; set; } = [];

    [Attr]
    public List<string> Tags { get; set; } = [];

    [HasOne]
    public Company? Employer { get; set; }
}
```

See [attributes](~/usage/resources/attributes.md#compound-attributes) for how to define compound attributes.

```http
GET /contacts?filter=equals(name.lastName,'Smith') HTTP/1.1
```

```http
GET /contacts?filter=startsWith(employer.address.city,'San') HTTP/1.1
```

Text matching and (ordered) comparisons work on nested attributes the same way as on top-level attributes.
A compound attribute or collection itself can only be compared with `null`, using `equals`:

```http
GET /contacts?filter=equals(name,null) HTTP/1.1
```

```http
GET /contacts?filter=not(equals(name.middleName,null)) HTTP/1.1
```

### Collections

The `has` function works on to-many relationships, scalar collections (such as `Tags`) and compound collections (such as `PhoneNumbers`).
Without a condition, it matches resources whose collection contains at least one element:

```http
GET /contacts?filter=has(phoneNumbers) HTTP/1.1
```

The `count` function works on to-many relationships and on both kinds of collections, also when nested in a compound attribute:

```http
GET /contacts?filter=greaterThan(count(tags),'2') HTTP/1.1
```

```http
GET /contacts?filter=equals(count(name.nicknames),'0') HTTP/1.1
```

> [!NOTE]
> A collection that is `null` counts as empty: `has` does not match it and `count` returns zero.

To match on the elements of a scalar collection, pass a condition to `has` and use the underscore (`_`) as the symbol for the current element.
It can be used in the comparison functions, `contains`, `startsWith`, `endsWith` and `any`, which can be combined with `not`, `and` and `or`:

```http
GET /contacts?filter=has(tags,equals(_,'vip')) HTTP/1.1
```

```http
GET /contacts?filter=has(tags,and(startsWith(_,'a'),not(contains(_,'z')))) HTTP/1.1
```

```http
GET /contacts?filter=has(tags,any(_,'vip','partner')) HTTP/1.1
```

The underscore can only be used as left operand (comparing an element with itself makes no sense) and only inside the condition of `has`.
An element of a scalar collection has no members, so `count`, `has` and `isType` cannot be used inside that condition.
Elements can be null when the element type allows it, so `has(tags,equals(_,null))` matches resources that contain a null element.

Inside the condition of `has` on a compound collection, the members of the element are in scope: names are resolved on the element instead
of on the resource being filtered, just like with `has` on a to-many relationship. Conditions can be nested.

```http
GET /contacts?filter=has(phoneNumbers,and(equals(kind,'Mobile'),startsWith(number,'+31'))) HTTP/1.1
```

`isType` cannot be used inside such a condition, because compound types do not support inheritance.

> [!NOTE]
> The `AllowFilter` capability is checked on every attribute and relationship in a chain, including compound attributes and the collection
> that is passed to `has` or `count`. Attributes (also those of compound types) are filterable by default.

> [!NOTE]
> Whether a particular filter on a compound attribute can be executed depends on the JSON support of the EF Core provider and the way the
> compound attribute is mapped.

_since v5.0_

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

# Legacy filters

The next section describes how filtering worked in versions prior to v4.0. They are always applied on the set of resources being requested (no nesting).
Legacy filters use the following form.

```
?filter[attribute]=value
```

For operations other than equality, the query can be prefixed with an operation identifier.
Examples can be found in the table below.

| Operation                     | Prefix           | Example                                         | Equivalent form in v4.0                               |
|-------------------------------|------------------|-------------------------------------------------|-------------------------------------------------------|
| Equality                      | `eq`             | `?filter[lastName]=eq:Smith`                    | `?filter=equals(lastName,'Smith')`                    |
| Non-equality                  | `ne`             | `?filter[lastName]=ne:Smith`                    | `?filter=not(equals(lastName,'Smith'))`               |
| Less than                     | `lt`             | `?filter[age]=lt:25`                            | `?filter=lessThan(age,'25')`                          |
| Less than or equal to         | `le`             | `?filter[lastModified]=le:2001-01-01`           | `?filter=lessOrEqual(lastModified,'2001-01-01')`      |
| Greater than                  | `gt`             | `?filter[duration]=gt:6:12:14`                  | `?filter=greaterThan(duration,'6:12:14')`             |
| Greater than or equal to      | `ge`             | `?filter[percentage]=ge:33.33`                  | `?filter=greaterOrEqual(percentage,'33.33')`          |
| Contains text                 | `like`           | `?filter[description]=like:cooking`             | `?filter=contains(description,'cooking')`             |
| Equals one value from set     | `in`             | `?filter[chapter]=in:Intro,Summary,Conclusion`  | `?filter=any(chapter,'Intro','Summary','Conclusion')` |
| Equals none from set          | `nin`            | `?filter[chapter]=nin:one,two,three`            | `?filter=not(any(chapter,'one','two','three'))`       |
| Equal to null                 | `isnull`         | `?filter[lastName]=isnull:`                     | `?filter=equals(lastName,null)`                       |
| Not equal to null             | `isnotnull`      | `?filter[lastName]=isnotnull:`                  | `?filter=not(equals(lastName,null))`                  |

Filters can be combined and will be applied using an OR operator. This used to be AND in versions prior to v4.0.

Attributes to filter on can optionally be prefixed with to-one relationships and (since v6.0) compound attributes, for example:

```http
GET /api/articles?include=author&filter[caption]=like:marketing&filter[author.lastName]=Smith HTTP/1.1
```

The `isnull:` and `isnotnull:` prefixes can also be used on to-one relationships and compound attributes, for example `?filter[calendar.workweek]=isnull:`.
Functions such as `has`, `count` and the element symbol `_` are only available in expression notation.

Legacy filter notation can still be used since v4.0 by setting `options.EnableLegacyFilterNotation` to `true`.
If you want to use the new filter notation in that case, prefix the parameter value with `expr:`, for example:

```http
GET /articles?filter[caption]=tech&filter=expr:equals(caption,'cooking')) HTTP/1.1
```

# Custom Filters

There are multiple ways you can add custom filters:

1. Implementing `IResourceDefinition.OnApplyFilter` (see [here](~/usage/extensibility/resource-definitions.md#change-filters)) and inject `IRequestQueryStringAccessor`, which works at all depths, but filter operations are constrained to what `FilterExpression` provides
2. Implementing `IResourceDefinition.OnRegisterQueryableHandlersForQueryStringParameters` as described [here](~/usage/extensibility/resource-definitions.md#custom-query-string-parameters), which enables the full range of `IQueryable<T>` functionality, but only works on primary endpoints
3. Add an implementation of `IQueryConstraintProvider` to supply additional `FilterExpression`s, which are combined with existing filters using AND operator
4. Override `EntityFrameworkCoreRepository.ApplyQueryLayer` to adapt the `IQueryable<T>` expression just before execution
5. Take a deep dive and plug into reader/parser/tokenizer/visitor/builder for adding additional general-purpose filter operators

# Filter syntax

For reference, we provide the EBNF grammar for filter expressions below (in [ANTLR4](https://github.com/antlr/antlr4) style):

```ebnf
grammar Filter;

filterExpression:
    notExpression
    | logicalExpression
    | comparisonExpression
    | matchTextExpression
    | anyExpression
    | hasExpression
    | isTypeExpression;

notExpression:
    'not' LPAREN filterExpression RPAREN;

logicalExpression:
    ( 'and' | 'or' ) LPAREN filterExpression ( COMMA filterExpression )* RPAREN;

// Only 'equals' accepts compound attributes, collections and to-one relationships (with 'null').
// The element symbol is only valid as left operand, inside the condition of 'has' on a scalar collection.
comparisonExpression:
    ( 'equals' | 'greaterThan' | 'greaterOrEqual' | 'lessThan' | 'lessOrEqual' ) LPAREN (
        countExpression | fieldChain | elementExpression
    ) COMMA (
        countExpression | literalConstant | 'null' | fieldChain
    ) RPAREN;

matchTextExpression:
    ( 'contains' | 'startsWith' | 'endsWith' ) LPAREN ( fieldChain | elementExpression ) COMMA literalConstant RPAREN;

anyExpression:
    'any' LPAREN ( fieldChain | elementExpression ) ( COMMA literalConstant )+ RPAREN;

// The condition is evaluated in the scope of the collection element: the related resource of a to-many relationship,
// the compound element of a compound collection, or the element symbol for a scalar collection.
hasExpression:
    'has' LPAREN fieldChain ( COMMA filterExpression )? RPAREN;

// Only available in the scope of a resource type.
isTypeExpression:
    'isType' LPAREN fieldChain? COMMA FIELD ( COMMA filterExpression )? RPAREN;

// Not available in the scope of a scalar collection element.
countExpression:
    'count' LPAREN fieldChain RPAREN;

elementExpression:
    '_';

fieldChain:
    FIELD ( '.' FIELD )*;

literalConstant:
    ESCAPED_TEXT;

LPAREN: '(';
RPAREN: ')';
COMMA: ',';

fragment OUTER_FIELD_CHARACTER: [A-Za-z0-9];
fragment INNER_FIELD_CHARACTER: [A-Za-z0-9_-];
FIELD: OUTER_FIELD_CHARACTER ( INNER_FIELD_CHARACTER* OUTER_FIELD_CHARACTER )?;

ESCAPED_TEXT: '\'' ( ~['] | '\'\'' )* '\'' ;

LINE_BREAKS: [\r\n]+ -> skip;
```
