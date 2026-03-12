using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ShareConfiguration : IEntityTypeConfiguration<Share>
    {
        public void Configure(EntityTypeBuilder<Share> builder)
        {
            builder.ToTable("Shares");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasMaxLength(36);
            
            builder.Property(s => s.QuotaNumber)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(s => s.ShareAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(s => s.IsPaid)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(s => s.IsDelayed)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(s => s.DatePay)
                .IsRequired()
                .HasColumnType("date"); // Se pone solo el tipo date ya que la fecha puede ser manipulada
                // o calcula ya sea por el usuario o el sistema haciendolo un calculo

            builder.HasOne(s => s.Loan)
                .WithMany(s => s.Shares)
                .HasForeignKey(s => s.LoanId);
                /* .OnDelete(DeleteBehavior.Restrict); No puese la forma de borrado en esta parte ya que no es necesario
                y si pones borrados diferentes en entidades, por que luego te puede lanzar un excepcion  */           
        }
    }
}