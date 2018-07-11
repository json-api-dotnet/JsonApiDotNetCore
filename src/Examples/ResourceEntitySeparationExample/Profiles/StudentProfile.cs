using AutoMapper;
using JsonApiDotNetCoreExample.Models;

namespace ResourceEntitySeparationExample.Profiles
{
    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            CreateMap<StudentDto, StudentEntity>()
                .ForMember(e => e.FirstName, opt => opt.MapFrom(d => StringSplit(d.Name, " ", 0)))
                .ForMember(e => e.LastName, opt => opt.MapFrom(d => StringSplit(d.Name, " ", 1)));
            CreateMap<StudentEntity, StudentDto>()
                .ForMember(d => d.Name, opt => opt.MapFrom(e => e.FirstName + " " + e.LastName));
        }

        private string StringSplit(string value, string split, int pos)
        {
            if (value == null)
            {
                return null;
            }

            var pieces = value.Split(split);
            if (pieces.Length < pos+1)
            {
                return null;
            }

            return pieces[pos];
        }
    }
}
