using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SavingsAccountConfiguration : IEntityTypeConfiguration<SavingsAccount>
    {
        public void Configure(EntityTypeBuilder<SavingsAccount> builder)
        {
            builder.ToTable("SavingsAccounts");

            builder.HasKey(x => x.Id);

            builder.Property(s => s.Id)
                .HasMaxLength(36);

            builder.Property(s => s.AccountNumber)
                .HasMaxLength(9)
                .IsRequired();

            builder.Property(s => s.Balance)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.IsPrincipal)
                .IsRequired();

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasMany(s => s.Transactions)
                .WithOne(s => s.SavingsAccount)
                .HasForeignKey(s => s.SavingAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()"); // Le puse el valor por defecto ya que son fechas que indican
                // el momento exacto en que algo ocurrio

            builder.HasIndex(s => s.AccountNumber).IsUnique();

            builder.Property(s => s.AdminId)
                .HasMaxLength(36)
                .IsRequired(false);

        }
    }
}