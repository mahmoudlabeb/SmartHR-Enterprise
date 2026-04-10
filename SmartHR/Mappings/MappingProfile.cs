using AutoMapper;
using SmartHR.Models;
using SmartHR.ViewModels;

namespace SmartHR.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Employee Mappings
            CreateMap<Employee, EmployeeEditViewModel>().ReverseMap();
            CreateMap<Employee, EmployeeCreateViewModel>().ReverseMap();
            
            // Add other mappings as needed
        }
    }
}
