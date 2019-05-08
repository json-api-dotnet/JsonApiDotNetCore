namespace JsonApiDotNetCore.Services
{

    /// <summary>
    /// A enum that represent the available resource hooks.
    /// </summary>
    public enum ResourceHook
    {
        None, // https://stackoverflow.com/questions/24151354/is-it-a-good-practice-to-add-a-null-or-none-member-to-the-enum
        BeforeCreate,
        AfterCreate,
        BeforeRead,
        AfterRead,
        BeforeUpdate,
        AfterUpdate,
        BeforeDelete,
        AfterDelete,
        BeforeUpdateRelationship,
        AfterUpdateRelationship
    }

}