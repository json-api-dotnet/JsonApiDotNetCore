namespace JsonApiDotNetCore.Hooks
{

    /// <summary>
    /// A enum that represent the available resource hooks.
    /// </summary>
    public enum ResourceHook
    {
        None, // https://stackoverflow.com/questions/24151354/is-it-a-good-practiceTo-add-a-null-or-noneMemberTo-the-enum
        BeforeCreate,
        BeforeRead,
        BeforeUpdate,
        BeforeDelete,
        BeforeUpdateRelationship,
        BeforeImplicitUpdateRelationship,
        OnReturn,
        AfterCreate,
        AfterRead,
        AfterUpdate,
        AfterDelete,
        AfterUpdateRelationship,
    }

}