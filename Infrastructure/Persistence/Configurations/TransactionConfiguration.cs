using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasMaxLength(36);

            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.Type)
                .HasDefaultValue(TransactionType.Deposit)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(t => t.Origin)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(t => t.Beneficiary)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(t => t.Status)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(t => t.Concept)
                .HasMaxLength(100);

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            builder.HasOne(t => t.SavingsAccount)
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.SavingAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}