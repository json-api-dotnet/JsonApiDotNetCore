using System.Collections.Generic;
using AutoMapper;
using JsonApiDotNetCoreExample.Models.Entities;
using JsonApiDotNetCoreExample.Models.Resources;

namespace ResourceEntitySeparationExample.Profiles
{
    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            CreateMap<StudentEntity, StudentResource>()
                .ForMember(d => d.Courses, opt => opt.MapFrom(e => CoursesFromRegistrations(e.Courses)))
                ;

            CreateMap<StudentResource, StudentEntity>()
                .ForMember(e => e.Courses, opt =>
                {
                });
        }

        private ICollection<CourseResource> CoursesFromRegistrations(ICollection<CourseStudentEntity> registrations)
        {
            ICollection<CourseResource> courses = new HashSet<CourseResource>();
            foreach (CourseStudentEntity reg in registrations)
            {
                CourseEntity e = reg.Course;
                courses.Add(new CourseResource
                {
                    Id = e.Id,
                    Number = e.Number,
                    Title = e.Title,
                    Description = e.Description
                });
            }
            return courses.Count == 0 ? null : courses;
        }
    }
}
