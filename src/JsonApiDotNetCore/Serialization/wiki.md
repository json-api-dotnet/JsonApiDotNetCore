# Architectual overview of serializers and deserializers

## Deserialization

When deserializing a json:api `Document` ([see document spec](https://jsonapi.org/format/#document-structure)), some parts are relevant only for client-side parsing whereas others are only for server-side parsing. Eg. `Document.Included` is only ever parsed by clients. Other parts are used by both. Therefore, the `JsonApiDeSerializer` implementation is now split into a `ServerDeserializer` and `ClientDeserializer`. Both inherit from `DocumentParser` which does the shared parsing.

#### DocumentParser
Responsible for 
- Converting the serialized string content into an intance of the `Document` class. 
- Building instances of the corresponding resource class (eg `Article`) by going through the document's primary data (`Document.Data`, [see primary data spec](https://jsonapi.org/format/#document-top-level)).

This base document parser is NOT responsible for any parsing that is unique to only client or server side parsing. That responsibility has been moved to its respective implementation through the abstract `DocumentParser.AfterProcessField()` method. This method is fired once each time after a `AttrAttribute` or `RelationshipAttribute` is processed. It allows a implementation of `DocumentParser` to intercept and complement the parsing of a resource with additional logic that is required for that specific implementation.

#### ClientDeserializer
The client deserializer complements the base deserialization  by
* overriding the `AfterProcessField` method which takes care of the Included section
	* after a relationship was deserialized, it finds the appended included object and adds it attributs and (nested) relationships
* taking care of remaining top-level members. These are members of a json:api `Document` that will only ever be relevant to a client-side parser:
	* Top-level meta data (`Document.Meta`)
	* Server-side errors (`Document.Errors`)

#### ServerDeserializer
For server-side parsing, no extra parsing needs to be done after the base deserialization is complerted. It only needs to keep track of which `AttrAttribute`s and `RelationshipAttribute`s were targeted by a request. This is needed for the internals of JADNC (eg the repository layer).
* The `AfterProcessField` method is overriden so that every attribute and relationship is registered with the `IUpdatedFields` service after it is processed.

## Serialization
Like with the deserializers, `JsonApiSerializer` is now split up into a `ServerSerializer` and `ClientSerializer`. Both inherit from a shared `DocumentBuilder` class. Additionally, `DocumentBuilder` inherits from `ResourceObjectBuilder`, which is extended by `IncludedResourceObjectBuilder`.

### ResourceObjectBuilder
At the core of serialization is the `ResourceObject` class [see resource object spec](https://jsonapi.org/format/#document-resource-objects).

ResourceObjectBuilder is responsible for 
- Building a `ResourceObject` from an entity given a list of `AttrAttribute`s and `RelationshipAttribute`s.
	- Note: the resource object builder is NOT responsible for figuring out which attributes and relationships should be included in the serialization result, because this differs depending on an the implementation being client or server side.
	  Instead, it is provided with the list.

Additionally, client and server serializers also differ in how relationship members ([see relationship member spec](https://jsonapi.org/format/#document-resource-object-attributes) are formatted. The responsibility for this handling is moved to the respective implementation, this time by overriding the `ResourceObjectBuilder.GetRelationshipData()` method. This method is fired once each time a `RelationshipAttribute` is processed, allowing for additional serialization (like adding links or metadata).

This time, the `GetRelationshipData()` method is not abstract, but virtual with a default implementation. This default implementation is to just create a `RelationshipData` with primary data (like `{"related-foo": { "data": { "id": 1" "type": "foobar"}}}`)

### DocumentBuilder
Responsible for
- Calling the base resource object serialization for one (or many) entities and wrapping the result in a `Document`.

Thats all. It does not figure out which attributes or relationships are to be serialized.

### ClientSerializer
Responsible for figuring out which attributes and relationships need to be serialized and calling the base document builder with that.
For example:
- for a POST request, this is often (almost) all attributes.
- for a PATCH request, this is usually a small subset of attributes.

Note that the client serializer is relatively skinny, because no top-level data (included, meta, links) will ever have to be added anywhere in the document.

### ServerSerializer
Responsible for figuring out which attributes and relationships need to be serialized and calling the base document builder with that.
For example, for a GET request, all attributes are usually included in the output, unless
- Sparse field selection was applied in the client request
- Runtime attribute hiding was applied, see [JADNC docs](https://json-api-dotnet.github.io/JsonApiDotNetCore/usage/resources/resource-definitions.html#runtime-attribute-filtering)

The server serializer is also responsible for adding top-level meta data and links and appending included relationships. For this the `GetRelationshipData()` is overriden:
- it adds links to the `RelationshipData` object (if configured to do so, see `ILinksConfiguration`).
- it checks if the processed relationship needs to be enclosed in the `included` list. If so, it calls the `IIncludedResourceObjectBuilder` to take care of that.


### IncludedResourceObjectBuilder
Responsible for building the *included member* of a `Document`. Note that `IncludedResourceObjectBuilder` extends `ResourceObjectBuilder` and not `DocumentBuilder` because it does not need to build an entire document but only resource objects.

Relationship *inclusion chains* are at the core of building the included member. For example, consider the request `articles?included=author.blogs.reviewers.favorite-food,reviewer.blogs.author.favorite-song`. It contains the following (complex) inclusion chains:
1. `author.blogs.reviewers.favorite-food`
2. `reviewer.blogs.author.favorite-song`

Like with the `ClientSerializer` and `ServerSerializer`, the `IncludedResourceObjectBuilder` is responsible for calling the base resource object builder with the list of attributes and relationships. For this implementation, these lists depend strongly on the inclusion chains. The above complex example demonstrates this (note: in this example the relationships `author` and `reviewer` are of the same resource `people`):
- people that were included as reviewers from inclusion chain (1) should come with their `favorite-food` included, but not those from chain (2)
- people that were included as authors from inclusion chain (2) should come with their `favorite-song` included, but not those from chain (1).
- a person that was included as both an reviewer and author (i.e. targeted by both chain (1) and (2)), both `favorite-food` and `favorite-song` need to be present.