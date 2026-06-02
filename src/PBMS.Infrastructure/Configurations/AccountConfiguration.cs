using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("account");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasColumnName("account_id")
                .ValueGeneratedOnAdd();

            builder.Property(a => a.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            builder.Property(a => a.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(a => a.Username)
                .IsUnique();

            builder.Property(a => a.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(100);

            builder.Property(a => a.Email)
                .HasColumnName("email")
                .HasMaxLength(100);

            builder.HasIndex(a => a.Email)
                .IsUnique();

            builder.Property(a => a.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20);

            builder.Property(a => a.AccountStatus)
                .HasColumnName("account_status")
                .HasMaxLength(20)
                .HasDefaultValue("Active")
                .IsRequired();

            // BaseEntity CreatedAt mapping
            builder.Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // Relationships
            builder.HasOne(a => a.Role)
                .WithMany(r => r.Accounts)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
