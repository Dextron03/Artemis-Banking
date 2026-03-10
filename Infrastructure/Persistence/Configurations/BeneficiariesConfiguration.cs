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
    public class BeneficiariesConfiguration : IEntityTypeConfiguration<Beneficiary>
    {
        public void Configure(EntityTypeBuilder<Beneficiary> builder)
        {
            builder.ToTable("Beneficiaries");

            builder
                .HasKey(b => b.Id); // Llave primaria

            builder.Property(b => b.Id)
                .HasMaxLength(36);

            builder.Property(b => b.AccountNumber)
                .IsRequired()
                .HasMaxLength(9);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(75);

            builder.Property(b => b.LastName)
                .IsRequired()
                .HasMaxLength(75);

            builder.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}