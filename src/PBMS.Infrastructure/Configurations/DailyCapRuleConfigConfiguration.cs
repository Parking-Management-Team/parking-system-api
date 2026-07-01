using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class DailyCapRuleConfigConfiguration : IEntityTypeConfiguration<DailyCapRuleConfig>
{
    public void Configure(EntityTypeBuilder<DailyCapRuleConfig> builder)
    {
        builder.ToTable("daily_cap_rule_config");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("daily_cap_rule_config_id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.PricingRuleId)
            .HasColumnName("pricing_rule_id")
            .IsRequired();

        builder.Property(c => c.MaximumDailyAmount)
            .HasColumnName("maximum_daily_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.CurrencyCode)
            .HasColumnName("currency_code")
            .HasMaxLength(10)
            .HasDefaultValue("VND")
            .IsRequired();

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Quan hệ 1-1 với PricingRule
        builder.HasOne(c => c.PricingRule)
            .WithOne(r => r.DailyCapRuleConfig)
            .HasForeignKey<DailyCapRuleConfig>(c => c.PricingRuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
