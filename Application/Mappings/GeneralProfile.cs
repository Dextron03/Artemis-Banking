using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.ViewModels;
using AutoMapper;
using Microsoft.Extensions.Hosting;
using static System.Collections.Specialized.BitVector32;

namespace Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            // AUTH
            CreateMap<LoginViewModel, LoginDto>();
        }
    }
}
