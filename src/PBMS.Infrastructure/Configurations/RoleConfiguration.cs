using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("role");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasColumnName("role_id")
                .ValueGeneratedOnAdd();

            builder.Property(r => r.RoleName)
                .HasColumnName("role_name")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(r => r.RoleName)
                .IsUnique();

            builder.Property(r => r.Description)
                .HasColumnName("description")
                .HasMaxLength(100);

            // BaseEntity CreatedAt mapping
            builder.Property(r => r.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        }
    }
}
