using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class GracePeriodRuleConfigConfiguration : IEntityTypeConfiguration<GracePeriodRuleConfig>
{
    public void Configure(EntityTypeBuilder<GracePeriodRuleConfig> builder)
    {
        builder.ToTable("grace_period_rule_config");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("grace_period_rule_config_id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.PricingRuleId)
            .HasColumnName("pricing_rule_id")
            .IsRequired();

        builder.Property(c => c.GracePeriodMinutes)
            .HasColumnName("grace_period_minutes")
            .IsRequired();

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Quan hệ 1-1 với PricingRule
        builder.HasOne(c => c.PricingRule)
            .WithOne(r => r.GracePeriodRuleConfig)
            .HasForeignKey<GracePeriodRuleConfig>(c => c.PricingRuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
