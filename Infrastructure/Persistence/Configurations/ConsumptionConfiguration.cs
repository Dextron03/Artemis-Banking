using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Infrastructure.Persistence.Configurations
{
    public class ConsumptionConfiguration : IEntityTypeConfiguration<Consumption>
    {
        
        public void Configure(EntityTypeBuilder<Consumption> builder)
        {
            builder.ToTable("Consumptions");

            builder.HasKey(c => c.Id); // Llave primaria

            builder.Property(c => c.Id)
                .HasMaxLength(36);

            builder.Property(c => c.ConsumptionDate)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.Status)
                .IsRequired()
                .HasDefaultValue(ConsumptionStatus.Rechazado)
                .HasConversion<string>();

            builder.Property(c => c.CommerceName)
                .IsRequired()
                .HasMaxLength(75);

            builder.HasOne(c => c.CreditCard)
                .WithMany(cc => cc.Consumptions)
                .HasForeignKey(c => c.CreditCardId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }   
}