using Application.DTOs.Dashboard;
using Application.DTOs.Login;
using Application.ViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            // AUTH
            CreateMap<LoginViewModel, LoginDto>();
            //Dashboard Admin
            CreateMap<DashboardAdminDto, DashboardAdminViewModel>();
            // Loan
            CreateMap<Loan, LoanViewModel>()
                .ForMember(d => d.TotalInstallments, o => o.MapFrom(s => s.Shares.Count))
                .ForMember(d => d.PaidInstallments, o => o.MapFrom(s => s.Shares.Count(x => x.IsPaid)))
                .ForMember(d => d.PendingAmount, o => o.MapFrom(s => s.OutstandingAmount))
                .ForMember(d => d.Months, o => o.MapFrom(s => s.TermMonths))
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()));
        }
    }
}
