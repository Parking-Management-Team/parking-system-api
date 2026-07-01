using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> builder)
    {
        builder.ToTable("pricing_rule");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("pricing_rule_id")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.PricingPolicyId)
            .HasColumnName("pricing_policy_id")
            .IsRequired();

        builder.Property(r => r.RuleType)
            .HasColumnName("rule_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.ExecutionOrder)
            .HasColumnName("execution_order")
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Mối quan hệ N-1 với PricingPolicy
        builder.HasOne(r => r.PricingPolicy)
            .WithMany(p => p.PricingRules)
            .HasForeignKey(r => r.PricingPolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
