using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models.Resources
{
    /// <summary>
    /// Note: EF Core *requires* the creation of an additional entity
    /// for many to many relationships and no longer implicitly creates
    /// it. While it may not make sense to create a corresponding "resource"
    /// for that relationship, due to the need to make the underlying 
    /// framework and mapping understand the explicit navigation entity, 
    /// a mirroring DTO resource is also required.
    /// </summary>
    public class CourseStudentResource : Identifiable
    {
        [HasOne("course")]
        public CourseResource Course { get; set; }
        public int CourseId { get; set; }

        [HasOne("student")]
        public StudentResource Student { get; set; }
        public int StudentId { get; set; }
    }
}
