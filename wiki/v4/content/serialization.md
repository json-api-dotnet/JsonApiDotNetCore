
# Serialization

The main change for serialization is that we have split the serialization responsibilities into two parts:

* **Response (de)serializers** - (de)Serialization regarding serving or interpreting a response.
* **Request (de)serializer** - (de)Serialization regarding creating or interpreting a request.

This split is done because during deserialization, some parts are relevant only for *client*-side parsing whereas others are only for *server*-side parsing. for example, a server deserializer will never have to deal with a `included` object list. Similarly, in serialization, a client serializer will for example never ever have to populate any other top-level members than the primary data (like `meta`, `included`). 

Throughout the document and the code when referring to fields, members, object types, the technical language of JSON:API spec is used. At the core of (de)serialization is the
`Document` class, [see document spec](https://jsonapi.org/format/#document-structure).

## Changes

In this section we will detail the changes made to the (de)serialization compared to the previous version.

### Deserialization

The previous `JsonApiDeSerializer` implementation is now split into a `RequestDeserializer` and `ResponseDeserializer`. Both inherit from `BaseDocumentParser` which does the shared parsing.

#### BaseDocumentParser

This (base) class is responsible for:

* Converting the serialized string content into an intance of the `Document` class. Which is the most basic version of JSON:API which has a `Data`, `Meta` and `Included` property.
* Building instances of the corresponding resource class (eg `Article`) by going through the document's primary data (`Document.Data`) For the spec for this: [Document spec](https://jsonapi.org/format/#document-top-level).

Responsibility of any implementation the base class-specific parsing is shifted through the abstract `BaseDocumentParser.AfterProcessField()` method. This method is fired once each time after a `AttrAttribute` or `RelationshipAttribute` is processed. It allows a implementation of `BaseDocumentParser` to intercept the parsing and add steps that are only required for new implementations.

#### ResponseDeserializer

The client deserializer complements the base deserialization by

* overriding the `AfterProcessField` method which takes care of the Included section \* after a relationship was deserialized, it finds the appended included object and adds it attributs and (nested) relationships
* taking care of remaining top-level members. that are only relevant to a client-side parser (meta data, server-side errors, links).

#### RequestDeserializer

For server-side parsing, no extra parsing needs to be done after the base deserialization is completed. It only needs to keep track of which `AttrAttribute`s and `RelationshipAttribute`s were targeted by a request. This is needed for the internals of JADNC (eg the repository layer).

* The `AfterProcessField` method is overriden so that every attribute and relationship is registered with the `ITargetedFields` service after it is processed.

## Serialization

Like with the deserializers, `JsonApiSerializer` is now split up into these classes (indentation implies hierarchy/extending):

* `IncludedResourceObjectBuilder`

* `ResourceObjectBuilder` - *abstract* 
  * `DocumentBuilder` - *abstract* -
    * `ResponseSerializer`
    * `RequestSerializer`

### ResourceObjectBuilder

At the core of serialization is the `ResourceObject` class [see resource object spec](https://jsonapi.org/format/#document-resource-objects).

ResourceObjectBuilder is responsible for Building a `ResourceObject` from an entity given a list of `AttrAttribute`s and `RelationshipAttribute`s. - Note: the resource object builder is NOT responsible for figuring out which attributes and relationships should be included in the serialization result, because this differs depending on an the implementation being client or server side. Instead, it is provided with the list.

Additionally, client and server serializers also differ in how relationship members ([see relationship member spec](https://jsonapi.org/format/#document-resource-object-attributes) are formatted. The responsibility for handling this is again shifted, this time by virtual `ResourceObjectBuilder.GetRelationshipData()` method. This method is fired once each time a `RelationshipAttribute` is processed, allowing for additional serialization (like adding links or metadata).

This time, the `GetRelationshipData()` method is not abstract, but virtual with a default implementation. This default implementation is to just create a `RelationshipData` with primary data (like `{"related-foo": { "data": { "id": 1" "type": "foobar"}}}`). Some implementations (server, included builder) need additional logic, others don't (client).

### BaseDocumentBuilder
Responsible for

-   Calling the base resource object serialization for one (or many) entities and wrapping the result in a `Document`.

Thats all. It does not figure out which attributes or relationships are to be serialized.

### RequestSerializer

Responsible for figuring out which attributes and relationships need to be serialized and calling the base document builder with that.
For example:

-   for a POST request, this is often (almost) all attributes.
-   for a PATCH request, this is usually a small subset of attributes.

Note that the client serializer is relatively skinny, because no top-level data (included, meta, links) will ever have to be added anywhere in the document.

### ResponseSerializer

Responsible for figuring out which attributes and relationships need to be serialized and calling the base document builder with that.
For example, for a GET request, all attributes are usually included in the output, unless

* Sparse field selection was applied in the client request
* Runtime attribute hiding was applied, see [JADNC docs](https://json-api-dotnet.github.io/JsonApiDotNetCore/usage/resources/resource-definitions.html#runtime-attribute-filtering)

The server serializer is also responsible for adding top-level meta data and links and appending included relationships. For this the `GetRelationshipData()` is overriden:

* it adds links to the `RelationshipData` object (if configured to do so, see `ILinksConfiguration`).
* it checks if the processed relationship needs to be enclosed in the `included` list. If so, it calls the `IIncludedResourceObjectBuilder` to take care of that.

### IncludedResourceObjectBuilder
Responsible for building the *included member* of a `Document`. Note that `IncludedResourceObjectBuilder` extends `ResourceObjectBuilder` and not `BaseDocumentBuilder` because it does not need to build an entire document but only resource objects.

Responsible for building the _included member_ of a `Document`. Note that `IncludedResourceObjectBuilder` extends `ResourceObjectBuilder` and not `DocumentBuilder` because it does not need to build an entire document but only resource objects.

Relationship _inclusion chains_ are at the core of building the included member. For example, consider the request `articles?included=author.blogs.reviewers.favorite-food,reviewer.blogs.author.favorite-song`. It contains the following (complex) inclusion chains:

1. `author.blogs.reviewers.favorite-food`
2. `reviewer.blogs.author.favorite-song`

Like with the `RequestSerializer` and `ResponseSerializer`, the `IncludedResourceObjectBuilder` is responsible for calling the base resource object builder with the list of attributes and relationships. For this implementation, these lists depend strongly on the inclusion chains. The above complex example demonstrates this (note: in this example the relationships `author` and `reviewer` are of the same resource `people`):

* people that were included as reviewers from inclusion chain (1) should come with their `favorite-food` included, but not those from chain (2)
* people that were included as authors from inclusion chain (2) should come with their `favorite-song` included, but not those from chain (1).
* a person that was included as both an reviewer and author (i.e. targeted by both chain (1) and (2)), both `favorite-food` and `favorite-song` need to be present.

To achieve this all of this, the `IncludedResourceObjectBuilder` needs to recursively parse an inclusion chain and make sure it does not append the same included more than once. This strategy is different from that of the ResponseSerializer, and for that reason it is a separate service.
