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
    public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            builder.ToTable("CreditCards", t =>
            {
                t.HasCheckConstraint("CK_CreditCard_DebtLimit", "[AmountDebt] <= [CreditLimit]");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasMaxLength(36);

            builder.Property(c => c.IdentifierNumber)
                .IsRequired()
                .HasMaxLength(16);

            builder.Property(c => c.CreditLimit)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.ExpireDate)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(c => c.AmountDebt)
                .IsRequired()
                .HasColumnType("decimal(18,2)");


            builder.Property(c => c.CvcCode)
                .IsRequired()
                .HasMaxLength(64);

            builder.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(c => c.IdentifierNumber).IsUnique();
        }
    }
}