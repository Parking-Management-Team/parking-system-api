using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class BasePricingRuleConfigConfiguration : IEntityTypeConfiguration<BasePricingRuleConfig>
{
    public void Configure(EntityTypeBuilder<BasePricingRuleConfig> builder)
    {
        builder.ToTable("base_pricing_rule_config");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("base_pricing_rule_config_id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.PricingRuleId)
            .HasColumnName("pricing_rule_id")
            .IsRequired();

        builder.Property(c => c.BaseDurationMinutes)
            .HasColumnName("base_duration_minutes")
            .IsRequired();

        builder.Property(c => c.BasePriceAmount)
            .HasColumnName("base_price_amount")
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
            .WithOne(r => r.BasePricingRuleConfig)
            .HasForeignKey<BasePricingRuleConfig>(c => c.PricingRuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
