namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Responsible for populating the RelationshipAttribute InverseNavigation property.
    /// 
    /// This service is instantiated in the configure phase of the application.
    /// 
    /// When using a data access layer different from EF Core, and when using ResourceHooks
    /// that depend on the inverse navigation property (BeforeImplicitUpdateRelationship),
    /// you will need to override this service, or pass along the inverseNavigationProperty in
    /// the RelationshipAttribute.
    /// </summary>
    public interface IInverseRelationships
    {
        /// <summary>
        /// This method is called upon startup by JsonApiDotNetCore. It should 
        /// deal with resolving the inverse relationships. 
        /// </summary>
        void Resolve();
    }
}
