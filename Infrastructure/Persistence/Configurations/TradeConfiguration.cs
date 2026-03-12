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
    public class TradeConfiguration : IEntityTypeConfiguration<Trade>
    {
        public void Configure(EntityTypeBuilder<Trade> builder)
        {
            builder.ToTable("Trades");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasMaxLength(36);

            builder.Property(t => t.Name)
                .HasMaxLength(75)
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.PathLogo)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.Status)
                .HasDefaultValue(TradeStatus.Rechazado)
                .HasConversion<string>()
                .IsRequired();

            builder.HasOne<AppUser>()
                .WithOne()
                .HasForeignKey<Trade>(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}