using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Models
{
    /// TODO: GetOutputAttrs is used in SERIALIATION LAYER to remove fields from
    /// list of attrs that will be displayed, (i.e. touches the DOCUMENT structure)
    /// whereas hooks is stuff to do on the MODEL in the SERVICELAYER.
    /// Consider (not sure yet) to move to different class because of this.
    /// edit: using different interfaces for this is maybe good enough to separate
    public interface ISerializableFields
    {
        List<AttrAttribute> GetAllowedAttributes(Type type);
        List<RelationshipAttribute> GetAllowedRelationships(Type type);
    }
}
