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
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.ToTable("Loans");

            builder.HasKey(x => x.Id);

            builder.Property(l => l.Id)
                .HasMaxLength(36);

            builder.Property(l => l.IdentifierNumber)
                .HasMaxLength(9)
                .IsRequired();

            builder.Property(l => l.LoanAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(l => l.TermMonths)
                .IsRequired();

            builder.Property(l => l.InterestRate)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            builder.Property(l => l.OutstandingAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(l => l.IsActive)
                .IsRequired();

            builder.Property(l => l.PaymentStatus)
                .IsRequired()
                .HasDefaultValue(PaymentStatusType.AlDia)
                .HasConversion<string>();

            builder.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Property(l => l.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.HasMany(x => x.Shares)
                .WithOne(s => s.Loan)
                .HasForeignKey(x=> x.LoanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}