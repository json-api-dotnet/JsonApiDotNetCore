# Examples

These requests have been generated against the "GettingStarted" application and are updated on every deployment.

All of these requests have been created using out-of-the-box features.

## Simple CRUD

### Create

[!code-sh[CREATE](000-CREATE_Person.sh)]
[!code-json[CREATE](000-CREATE_Person-Response.json)]

### Create with Relationship

[!code-sh[CREATE](001-CREATE_Article.sh)]
[!code-json[CREATE](001-CREATE_Article-Response.json)]


### Get All

[!code-sh[GET Request](002-GET_Articles.sh)]
[!code-json[GET Response](002-GET_Articles-Response.json)]

### Get By Id

[!code-sh[GET Request](003-GET_Article.sh)]
[!code-json[GET Response](003-GET_Article-Response.json)]

### Get with Relationship

[!code-sh[GET Request](004-GET_Articles_With_Authors.sh)]
[!code-json[GET Response](004-GET_Articles_With_Authors-Response.json)]

### Update

[!code-sh[PATCH Request](005-PATCH_Article.sh)]
[!code-json[PATCH Response](005-PATCH_Article-Response.json)]

### Delete

[!code-sh[DELETE Request](006-DELETE_Article.sh)]
[!code-json[DELETE Response](006-DELETE_Article-Response.json)]

## Filters

_Note that cURL requires URLs to be escaped._

### Equality

[!code-sh[GET Request](008-GET_Articles_With_Filter_Eq.sh)]
[!code-json[GET Response](008-GET_Articles_With_Filter_Eq-Response.json)]

### Like

[!code-sh[GET Request](009-GET_Articles_With_Filter_Like.sh)]
[!code-json[GET Response](009-GET_Articles_With_Filter_Like-Response.json)]

## Sorting

# See Also

- Customizing QuerySet
