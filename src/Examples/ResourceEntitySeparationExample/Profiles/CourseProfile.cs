using AutoMapper;
using JsonApiDotNetCoreExample.Models.Entities;
using JsonApiDotNetCoreExample.Models.Resources;
using System.Collections.Generic;

namespace ResourceEntitySeparationExample.Profiles
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<CourseEntity, CourseResource>()
                .ForMember(r => r.Students, opt => opt.MapFrom(e => StudentsFromRegistrations(e.Students)))
                .ForMember(r => r.Department, opt => opt.MapFrom(e => new DepartmentResource
                {
                    Id = e.Department.Id,
                    Name = e.Department.Name
                }))
                ;

            CreateMap<CourseResource, CourseEntity>()
                // .ForMember(e => e.Registrations, opt => opt.MapFrom(r => RegistrationsFromResource(r)))
                ;
        }

        private ICollection<StudentResource> StudentsFromRegistrations(ICollection<CourseStudentEntity> registrations)
        {
            ICollection<StudentResource> students = new HashSet<StudentResource>();
            foreach(CourseStudentEntity reg in registrations)
            {
                StudentEntity e = reg.Student;
                students.Add(new StudentResource
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Address = e.Address
                });
            }
            return students.Count == 0 ? null : students;
        }

        private ICollection<CourseStudentEntity> RegistrationsFromResource(CourseResource course)
        {
            ICollection<CourseStudentEntity> registrations = new HashSet<CourseStudentEntity>();
            foreach (StudentResource s in course.Students)
            {
                registrations.Add(new CourseStudentEntity(course.Id, s.Id));
            }
            return registrations.Count == 0 ? null : registrations;
        }
    }
}
