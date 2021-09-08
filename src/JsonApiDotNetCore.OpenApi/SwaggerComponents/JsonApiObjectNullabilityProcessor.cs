using Microsoft.OpenApi.Models;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    /// <summary>
    /// Removes unwanted nullability of entries in schemas of JSON:API documents.
    /// </summary>
    /// <remarks>
    /// Initially these entries are marked nullable by Swashbuckle because nullable reference types are not enabled. This post-processing step can be removed
    /// entirely once we enable nullable reference types. See eg
    /// https://github.com/degreed/JsonApiCorePrototype/blob/936db8950d925f1b8a055cf5d8bba753f6579094/src/Web/OpenApi/JsonApiObjects/Documents/ManyResourceIdentifierResponseDocument.cs#L7
    /// </remarks>
    internal sealed class JsonApiObjectNullabilityProcessor
    {
        private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;

        public JsonApiObjectNullabilityProcessor(ISchemaRepositoryAccessor schemaRepositoryAccessor)
        {
            ArgumentGuard.NotNull(schemaRepositoryAccessor, nameof(schemaRepositoryAccessor));

            _schemaRepositoryAccessor = schemaRepositoryAccessor;
        }

        public void ClearDocumentProperties(OpenApiSchema referenceSchemaForDocument)
        {
            ArgumentGuard.NotNull(referenceSchemaForDocument, nameof(referenceSchemaForDocument));

            OpenApiSchema fullSchemaForDocument = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForDocument.Reference.Id];

            ClearMetaObjectNullability(fullSchemaForDocument);
            ClearJsonapiObjectNullability(fullSchemaForDocument);
            ClearLinksObjectNullability(fullSchemaForDocument);

            OpenApiSchema fullSchemaForResourceObject = TryGetFullSchemaForResourceObject(fullSchemaForDocument);

            if (fullSchemaForResourceObject != null)
            {
                ClearResourceObjectNullability(fullSchemaForResourceObject);
            }
        }

        private static void ClearMetaObjectNullability(OpenApiSchema fullSchema)
        {
            if (fullSchema.Properties.ContainsKey(JsonApiObjectPropertyName.MetaObject))
            {
                fullSchema.Properties[JsonApiObjectPropertyName.MetaObject].Nullable = false;
            }
        }

        private void ClearJsonapiObjectNullability(OpenApiSchema fullSchema)
        {
            if (fullSchema.Properties.ContainsKey(JsonApiObjectPropertyName.JsonapiObject))
            {
                OpenApiSchema fullSchemaForJsonapiObject =
                    _schemaRepositoryAccessor.Current.Schemas[fullSchema.Properties[JsonApiObjectPropertyName.JsonapiObject].Reference.Id];

                fullSchemaForJsonapiObject.Properties[JsonApiObjectPropertyName.JsonapiObjectVersion].Nullable = false;
                fullSchemaForJsonapiObject.Properties[JsonApiObjectPropertyName.JsonapiObjectExt].Nullable = false;
                fullSchemaForJsonapiObject.Properties[JsonApiObjectPropertyName.JsonapiObjectProfile].Nullable = false;
                ClearMetaObjectNullability(fullSchemaForJsonapiObject);
            }
        }

        private void ClearLinksObjectNullability(OpenApiSchema fullSchema)
        {
            if (fullSchema.Properties.ContainsKey(JsonApiObjectPropertyName.LinksObject))
            {
                OpenApiSchema fullSchemaForLinksObject =
                    _schemaRepositoryAccessor.Current.Schemas[fullSchema.Properties[JsonApiObjectPropertyName.LinksObject].Reference.Id];

                foreach (OpenApiSchema schemaForEntryInLinksObject in fullSchemaForLinksObject.Properties.Values)
                {
                    schemaForEntryInLinksObject.Nullable = false;
                }
            }
        }

        private OpenApiSchema TryGetFullSchemaForResourceObject(OpenApiSchema fullSchemaForDocument)
        {
            OpenApiSchema schemaForDataObject = fullSchemaForDocument.Properties[JsonApiObjectPropertyName.Data];
            OpenApiReference dataSchemaReference = schemaForDataObject.Type == "array" ? schemaForDataObject.Items.Reference : schemaForDataObject.Reference;

            if (dataSchemaReference == null)
            {
                return null;
            }

            return _schemaRepositoryAccessor.Current.Schemas[dataSchemaReference.Id];
        }

        private void ClearResourceObjectNullability(OpenApiSchema fullSchemaForValueOfData)
        {
            ClearMetaObjectNullability(fullSchemaForValueOfData);
            ClearLinksObjectNullability(fullSchemaForValueOfData);
            ClearAttributesObjectNullability(fullSchemaForValueOfData);
            ClearRelationshipsObjectNullability(fullSchemaForValueOfData);
        }

        private void ClearAttributesObjectNullability(OpenApiSchema fullSchemaForResourceObject)
        {
            if (fullSchemaForResourceObject.Properties.ContainsKey(JsonApiObjectPropertyName.AttributesObject))
            {
                OpenApiSchema fullSchemaForAttributesObject = _schemaRepositoryAccessor.Current.Schemas[
                    fullSchemaForResourceObject.Properties[JsonApiObjectPropertyName.AttributesObject].Reference.Id];

                fullSchemaForAttributesObject.Nullable = false;
            }
        }

        private void ClearRelationshipsObjectNullability(OpenApiSchema fullSchemaForResourceObject)
        {
            if (fullSchemaForResourceObject.Properties.ContainsKey(JsonApiObjectPropertyName.RelationshipsObject))
            {
                OpenApiSchema fullSchemaForRelationshipsObject = _schemaRepositoryAccessor.Current.Schemas[
                    fullSchemaForResourceObject.Properties[JsonApiObjectPropertyName.RelationshipsObject].Reference.Id];

                fullSchemaForRelationshipsObject.Nullable = false;
                ClearRelationshipsDataNullability(fullSchemaForRelationshipsObject);
            }
        }

        private void ClearRelationshipsDataNullability(OpenApiSchema fullSchemaForRelationshipsObject)
        {
            foreach (OpenApiSchema relationshipObjectData in fullSchemaForRelationshipsObject.Properties.Values)
            {
                OpenApiSchema fullSchemaForRelationshipsObjectData = _schemaRepositoryAccessor.Current.Schemas[relationshipObjectData.Reference.Id];
                ClearLinksObjectNullability(fullSchemaForRelationshipsObjectData);
                ClearMetaObjectNullability(fullSchemaForRelationshipsObjectData);
            }
        }
    }
}
