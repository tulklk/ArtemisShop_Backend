using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtermisShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOrderNumberToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a new temporary column with text type
            migrationBuilder.AddColumn<string>(
                name: "OrderNumberNew",
                table: "Orders",
                type: "text",
                nullable: true);

            // Step 2: Generate new 7-digit order numbers for existing orders
            // Using a function to generate random 7-digit numbers
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    order_record RECORD;
                    new_order_number TEXT;
                    counter INT := 0;
                BEGIN
                    FOR order_record IN SELECT ""Id"" FROM ""Orders"" ORDER BY ""CreatedAt""
                    LOOP
                        -- Generate a unique 7-digit number (1000000 to 9999999)
                        LOOP
                            new_order_number := LPAD((FLOOR(RANDOM() * 9000000)::INT + 1000000)::TEXT, 7, '0');
                            EXIT WHEN NOT EXISTS (SELECT 1 FROM ""Orders"" WHERE ""OrderNumberNew"" = new_order_number);
                        END LOOP;
                        
                        UPDATE ""Orders""
                        SET ""OrderNumberNew"" = new_order_number
                        WHERE ""Id"" = order_record.""Id"";
                        
                        counter := counter + 1;
                    END LOOP;
                END $$;
            ");

            // Step 3: Set NOT NULL constraint on the new column
            migrationBuilder.AlterColumn<string>(
                name: "OrderNumberNew",
                table: "Orders",
                type: "text",
                nullable: false);

            // Step 4: Drop the old column
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Orders");

            // Step 5: Rename the new column to OrderNumber
            migrationBuilder.RenameColumn(
                name: "OrderNumberNew",
                table: "Orders",
                newName: "OrderNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a new temporary column with uuid type
            migrationBuilder.AddColumn<Guid>(
                name: "OrderNumberOld",
                table: "Orders",
                type: "uuid",
                nullable: true);

            // Step 2: Generate new GUIDs for existing orders
            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET ""OrderNumberOld"" = gen_random_uuid();
            ");

            // Step 3: Set NOT NULL constraint
            migrationBuilder.AlterColumn<Guid>(
                name: "OrderNumberOld",
                table: "Orders",
                type: "uuid",
                nullable: false);

            // Step 4: Drop the old column
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Orders");

            // Step 5: Rename the new column to OrderNumber
            migrationBuilder.RenameColumn(
                name: "OrderNumberOld",
                table: "Orders",
                newName: "OrderNumber");
        }
    }
}
