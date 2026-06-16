using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    public class RevenueStatisticConfiguration : IEntityTypeConfiguration<RevenueStatistic>
    {
        public void Configure(EntityTypeBuilder<RevenueStatistic> builder)
        {
            builder.ToTable("revenue_statistic");

            builder.HasKey(rs => rs.Id);

            builder.Property(rs => rs.Id)
                .HasColumnName("statistic_id")
                .ValueGeneratedOnAdd();

            builder.Property(rs => rs.StatDate)
                .HasColumnName("stat_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(rs => rs.VehicleTypeId)
                .HasColumnName("vehicle_type_id");

            builder.Property(rs => rs.PaymentMethod)
                .HasColumnName("payment_method")
                .HasMaxLength(20);

            builder.Property(rs => rs.TotalPaymentsCount)
                .HasColumnName("total_payments_count")
                .IsRequired();

            builder.Property(rs => rs.TotalRevenue)
                .HasColumnName("total_revenue")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(rs => rs.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(rs => rs.RowVersion)
                .IsRowVersion();

            builder.Property(rs => rs.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasOne(rs => rs.VehicleType)
                .WithMany()
                .HasForeignKey(rs => rs.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
