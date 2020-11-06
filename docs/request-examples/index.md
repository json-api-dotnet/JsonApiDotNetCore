# Example requests

These requests have been generated against the "GettingStarted" application and are updated on every deployment.

All of these requests have been created using out-of-the-box features.

_Note that cURL requires "[" and "]" in URLs to be escaped._

# Reading data

### Get all

[!code-ps[REQUEST](001_GET_Books.ps1)]
[!code-json[RESPONSE](001_GET_Books_Response.json)]

### Get by ID

[!code-ps[REQUEST](002_GET_Person-by-ID.ps1)]
[!code-json[RESPONSE](002_GET_Person-by-ID_Response.json)]

### Get with relationship

[!code-ps[REQUEST](003_GET_Books-including-Author.ps1)]
[!code-json[RESPONSE](003_GET_Books-including-Author_Response.json)]

### Get sparse fieldset

[!code-ps[REQUEST](004_GET_Books-PublishYear.ps1)]
[!code-json[RESPONSE](004_GET_Books-PublishYear_Response.json)]

### Filter on partial match

[!code-ps[REQUEST](005_GET_People-Filter_Partial.ps1)]
[!code-json[RESPONSE](005_GET_People-Filter_Partial_Response.json)]

### Sorting

[!code-ps[REQUEST](006_GET_Books-sorted-by-PublishYear-descending.ps1)]
[!code-json[RESPONSE](006_GET_Books-sorted-by-PublishYear-descending_Response.json)]

### Pagination

[!code-ps[REQUEST](007_GET_Books-paginated.ps1)]
[!code-json[RESPONSE](007_GET_Books-paginated_Response.json)]

# Writing data

### Create resource

[!code-ps[REQUEST](010_CREATE_Person.ps1)]
[!code-json[RESPONSE](010_CREATE_Person_Response.json)]

### Create resource with relationship

[!code-ps[REQUEST](011_CREATE_Book-with-Author.ps1)]
[!code-json[RESPONSE](011_CREATE_Book-with-Author_Response.json)]

### Update resource

[!code-ps[REQUEST](012_PATCH_Book.ps1)]
[!code-json[RESPONSE](012_PATCH_Book_Response.json)]

### Delete resource

[!code-ps[REQUEST](013_DELETE_Book.ps1)]
[!code-json[RESPONSE](013_DELETE_Book_Response.json)]
