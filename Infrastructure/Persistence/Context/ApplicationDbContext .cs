using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Beneficiary> Beneficiaries { get; set;}
        public DbSet<Consumption> Consumptions{ get; set;}
        public DbSet<CreditCard>  CreditCards { get; set;}
        public DbSet<Loan> Loans { get; set;}
        public DbSet<SavingsAccount> SavingsAccounts { get; set;}
        public DbSet<Share> Shares { get; set;}
        public DbSet<Trade> Trades { get; set;}
        public DbSet<Transaction> Transactions { get; set;}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}