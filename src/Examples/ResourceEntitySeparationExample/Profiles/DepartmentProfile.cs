using AutoMapper;
using JsonApiDotNetCoreExample.Models.Entities;
using JsonApiDotNetCoreExample.Models.Resources;

namespace ResourceEntitySeparationExample.Profiles
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            CreateMap<DepartmentEntity, DepartmentResource>();
        }
    }
}
