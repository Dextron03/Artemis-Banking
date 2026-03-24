using Application.DTOs.CreditCard;
using Application.DTOs.Dashboard;
using Application.DTOs.Loan;
using Application.DTOs.Login;
using Application.DTOs.SavingsAccount;
using Application.DTOs.User;
using Application.ViewModels.DashboardAdmin;
using Application.ViewModels.Loan;
using Application.ViewModels.Login;
using Application.ViewModels.SavingsAccount;
using Application.ViewModels.User;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity.Entities;
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
            // User
            CreateMap<AppUser, UserViewModel>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirtsName} {s.LastName}"))
                .ForMember(d => d.Cedula, o => o.MapFrom(s =>
                string.IsNullOrEmpty(s.IdentityNumber) ? "N/A" : s.IdentityNumber))
                .ForMember(d => d.Username, o => o.MapFrom(s => s.UserName ?? string.Empty))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Email ?? string.Empty))
                .ForMember(d => d.IsActive, o => o.MapFrom(s =>
            s.LockoutEnd == null || s.LockoutEnd <= DateTimeOffset.Now));

            CreateMap<SaveUserViewModel, CreateUserDto>()
                .ForMember(d => d.InitialAmount, o => o.MapFrom(s => s.InitialAmount ?? 0));
            CreateMap<SaveUserViewModel, UpdateUserDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id ?? string.Empty));
            // AUTH
            CreateMap<LoginViewModel, LoginDto>();
            //Dashboard Admin
            CreateMap<DashboardAdminDto, DashboardAdminViewModel>();
            // Loan
            CreateMap<LoanSummaryDto, LoanViewModel>();
            CreateMap<EligibleClientDto, ClientLoanViewModel>()
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.TotalDebt, o => o.MapFrom(s => s.TotalDebt));
            CreateMap<CreateLoanViewModel, CreateLoanDto>()
                .ForMember(d => d.AdminId, o => o.Ignore());
            CreateMap<EditLoanRateViewModel, UpdateLoanRateDto>()
                .ForMember(d => d.LoanId, o => o.MapFrom(s => s.LoanId))
                .ForMember(d => d.NewRate, o => o.MapFrom(s => s.InterestRate));
            // Share
            CreateMap<ShareDto, ShareViewModel>();
            // CreditCard
            CreateMap<CreditCard, CreditCardListDto>()
                .ForMember(d => d.ClientFullName, opt => opt.Ignore());
            CreateMap<CreditCard, CreditCardDetailDto>()
                .ForMember(d => d.ClientFullName, opt => opt.Ignore())
                .ForMember(d => d.Consumptions, opt => opt.MapFrom(src => src.Consumptions));
            // Consumption
            CreateMap<Consumption, ConsumptionDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            // SavingsAccount
            CreateMap<SavingsAccount, SavingsAccountListDto>()
                .ForMember(d => d.ClientFullName, opt => opt.Ignore());
            CreateMap<SavingsAccount, SavingsAccountRowViewModel>()
                .ForMember(d => d.ClientFullName, opt => opt.Ignore());
            CreateMap<SavingsAccount, SavingsAccountDetailViewModel>()
                .ForMember(d => d.ClientFullName, opt => opt.Ignore())
                .ForMember(d => d.Transactions, opt => opt.Ignore());
            CreateMap<SavingsAccount, CancelSavingsAccountViewModel>()
                .ForMember(d => d.ClientFullName, opt => opt.Ignore());
            CreateMap<SavingsAccountListDto, SavingsAccountRowViewModel>();
            CreateMap<CancelSavingsAccountInfoDto, CancelSavingsAccountViewModel>();
            // Transaction
            CreateMap<Transaction, TransactionRowViewModel>()
                .ForMember(d => d.TransactionType, opt => opt.Ignore());
            CreateMap<Transaction, TransactionDto>()
                .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));
        }
    }
}
