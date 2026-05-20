using AutoMapper;
using EmployeeApi.DTOs;
using EmployeeApi.Models;

namespace EmployeeApi.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Employee, EmployeeReadDto>();

            CreateMap<EmployeeCreateDto, Employee>();

            CreateMap<Employee, EmployeePatchDto>();

            CreateMap<EmployeePatchDto, Employee>();
        }
    }
}