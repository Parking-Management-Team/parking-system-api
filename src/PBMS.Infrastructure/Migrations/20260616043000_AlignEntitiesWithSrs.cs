using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PBMS.Infrastructure.Data;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    [Migration("20260616043000_AlignEntitiesWithSrs")]
    [DbContext(typeof(AppDbContext))]
    public partial class AlignEntitiesWithSrs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_building_building_code";
                ALTER TABLE building DROP COLUMN IF EXISTS building_code;

                DROP INDEX IF EXISTS "IX_card_rfid_code";
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'card' AND column_name = 'rfid_code'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'card' AND column_name = 'nfc_uid'
                    ) THEN
                        ALTER TABLE card RENAME COLUMN rfid_code TO nfc_uid;
                    END IF;
                END $$;
                ALTER TABLE card ADD COLUMN IF NOT EXISTS updated_at timestamp with time zone NULL;
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_card_nfc_uid" ON card (nfc_uid) WHERE nfc_uid IS NOT NULL;

                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'zone' AND column_name = 'Code'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'zone' AND column_name = 'zone_code'
                    ) THEN
                        ALTER TABLE zone RENAME COLUMN "Code" TO zone_code;
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'zone' AND column_name = 'name'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'zone' AND column_name = 'zone_name'
                    ) THEN
                        ALTER TABLE zone RENAME COLUMN name TO zone_name;
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'zone' AND column_name = 'status'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'zone' AND column_name = 'zone_status'
                    ) THEN
                        ALTER TABLE zone RENAME COLUMN status TO zone_status;
                    END IF;
                END $$;
                ALTER TABLE zone ADD COLUMN IF NOT EXISTS zone_access_type character varying(20) NOT NULL DEFAULT 'GENERAL';
                ALTER TABLE zone ALTER COLUMN zone_code TYPE character varying(20);
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_zone_floor_id_zone_code" ON zone (floor_id, zone_code);

                ALTER TABLE revenue_statistic DROP CONSTRAINT IF EXISTS "FK_revenue_statistic_building_building_id";
                DROP INDEX IF EXISTS "IX_revenue_statistic_building_id";
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS building_id;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS start_date;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS end_date;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS period_type;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS total_bookings;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS total_sessions;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS total_subscriptions;
                ALTER TABLE revenue_statistic ADD COLUMN IF NOT EXISTS stat_date date NOT NULL DEFAULT CURRENT_DATE;
                ALTER TABLE revenue_statistic ADD COLUMN IF NOT EXISTS vehicle_type_id integer NULL;
                ALTER TABLE revenue_statistic ADD COLUMN IF NOT EXISTS payment_method character varying(20) NULL;
                ALTER TABLE revenue_statistic ADD COLUMN IF NOT EXISTS total_payments_count integer NOT NULL DEFAULT 0;
                ALTER TABLE revenue_statistic ADD COLUMN IF NOT EXISTS updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                CREATE INDEX IF NOT EXISTS "IX_revenue_statistic_vehicle_type_id" ON revenue_statistic (vehicle_type_id);
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'FK_revenue_statistic_vehicle_type_vehicle_type_id'
                    ) THEN
                        ALTER TABLE revenue_statistic
                        ADD CONSTRAINT "FK_revenue_statistic_vehicle_type_vehicle_type_id"
                        FOREIGN KEY (vehicle_type_id) REFERENCES vehicle_type(vehicle_type_id)
                        ON DELETE RESTRICT;
                    END IF;
                END $$;

                ALTER TABLE payment DROP CONSTRAINT IF EXISTS "CK_Payment_Source";
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'CK_Payment_Source'
                    ) THEN
                        ALTER TABLE payment
                        ADD CONSTRAINT "CK_Payment_Source"
                        CHECK (
                            (CASE WHEN session_id IS NOT NULL THEN 1 ELSE 0 END) +
                            (CASE WHEN booking_id IS NOT NULL THEN 1 ELSE 0 END) +
                            (CASE WHEN monthly_subscription_id IS NOT NULL THEN 1 ELSE 0 END) = 1
                        );
                    END IF;
                END $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE payment DROP CONSTRAINT IF EXISTS "CK_Payment_Source";
                ALTER TABLE payment
                ADD CONSTRAINT "CK_Payment_Source"
                CHECK (session_id IS NOT NULL OR booking_id IS NOT NULL OR monthly_subscription_id IS NOT NULL);

                ALTER TABLE revenue_statistic DROP CONSTRAINT IF EXISTS "FK_revenue_statistic_vehicle_type_vehicle_type_id";
                DROP INDEX IF EXISTS "IX_revenue_statistic_vehicle_type_id";
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS stat_date;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS vehicle_type_id;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS payment_method;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS total_payments_count;
                ALTER TABLE revenue_statistic DROP COLUMN IF EXISTS updated_at;

                DROP INDEX IF EXISTS "IX_zone_floor_id_zone_code";
                ALTER TABLE zone DROP COLUMN IF EXISTS zone_access_type;

                DROP INDEX IF EXISTS "IX_card_nfc_uid";
                ALTER TABLE card DROP COLUMN IF EXISTS updated_at;
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'card' AND column_name = 'nfc_uid'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'card' AND column_name = 'rfid_code'
                    ) THEN
                        ALTER TABLE card RENAME COLUMN nfc_uid TO rfid_code;
                    END IF;
                END $$;
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_card_rfid_code" ON card (rfid_code) WHERE rfid_code IS NOT NULL;
                """);
        }
    }
}
