using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class IncrementPricingRuleConfigConfiguration : IEntityTypeConfiguration<IncrementPricingRuleConfig>
{
    public void Configure(EntityTypeBuilder<IncrementPricingRuleConfig> builder)
    {
        builder.ToTable("increment_pricing_rule_config");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("increment_pricing_rule_config_id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.PricingRuleId)
            .HasColumnName("pricing_rule_id")
            .IsRequired();

        builder.Property(c => c.IncrementIntervalMinutes)
            .HasColumnName("increment_interval_minutes")
            .IsRequired();

        builder.Property(c => c.IncrementPriceAmount)
            .HasColumnName("increment_price_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.ThresholdPercentage)
            .HasColumnName("threshold_percentage")
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
            .WithOne(r => r.IncrementPricingRuleConfig)
            .HasForeignKey<IncrementPricingRuleConfig>(c => c.PricingRuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
