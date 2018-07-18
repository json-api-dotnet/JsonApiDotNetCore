using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonApiDotNetCoreExample.Models.Entities
{
    [Table("CourseStudent")]
    public class CourseStudentEntity
    {
        private CourseEntity _course;
        private StudentEntity _student;
        private ILazyLoader _loader { get; set; }
        private CourseStudentEntity(ILazyLoader loader)
        {
            _loader = loader;
        }

        public CourseStudentEntity(int courseId, int studentId)
        {
            CourseId = courseId;
            StudentId = studentId;
        }

        public CourseStudentEntity(CourseEntity course, StudentEntity student)
        {
            Course = course;
            CourseId = course.Id;
            Student = student;
            StudentId = student.Id;
        }

        [Column("course_id")]
        public int CourseId { get; set; }

        public CourseEntity Course
        {
            get => _loader.Load(this, ref _course);
            set => _course = value;
        }

        [Column("student_id")]
        public int StudentId { get; set; }

        public StudentEntity Student
        {
            get => _loader.Load(this, ref _student);
            set => _student = value;
        }
    }
}
