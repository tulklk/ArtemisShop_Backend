using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtermisShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdentityTablesAndUpdateUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraints first
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- Drop foreign keys from UserRoles if exists
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_UserRoles_Users_UserId' AND table_name = 'UserRoles') THEN
                        ALTER TABLE ""UserRoles"" DROP CONSTRAINT ""FK_UserRoles_Users_UserId"";
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_UserRoles_Roles_RoleId' AND table_name = 'UserRoles') THEN
                        ALTER TABLE ""UserRoles"" DROP CONSTRAINT ""FK_UserRoles_Roles_RoleId"";
                    END IF;
                    
                    -- Drop foreign keys from UserClaims if exists
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_UserClaims_Users_UserId' AND table_name = 'UserClaims') THEN
                        ALTER TABLE ""UserClaims"" DROP CONSTRAINT ""FK_UserClaims_Users_UserId"";
                    END IF;
                    
                    -- Drop foreign keys from UserLogins if exists
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_UserLogins_Users_UserId' AND table_name = 'UserLogins') THEN
                        ALTER TABLE ""UserLogins"" DROP CONSTRAINT ""FK_UserLogins_Users_UserId"";
                    END IF;
                    
                    -- Drop foreign keys from UserTokens if exists
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_UserTokens_Users_UserId' AND table_name = 'UserTokens') THEN
                        ALTER TABLE ""UserTokens"" DROP CONSTRAINT ""FK_UserTokens_Users_UserId"";
                    END IF;
                    
                    -- Drop foreign keys from RoleClaims if exists
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_RoleClaims_Roles_RoleId' AND table_name = 'RoleClaims') THEN
                        ALTER TABLE ""RoleClaims"" DROP CONSTRAINT ""FK_RoleClaims_Roles_RoleId"";
                    END IF;
                END $$;
            ");

            // Drop Identity tables if they exist
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""UserRoles"" CASCADE;
                DROP TABLE IF EXISTS ""UserClaims"" CASCADE;
                DROP TABLE IF EXISTS ""UserLogins"" CASCADE;
                DROP TABLE IF EXISTS ""UserTokens"" CASCADE;
                DROP TABLE IF EXISTS ""RoleClaims"" CASCADE;
                DROP TABLE IF EXISTS ""Roles"" CASCADE;
            ");

            // Update Users table - drop Identity-related columns
            migrationBuilder.Sql(@"
                -- Remove Identity-related columns if they exist
                ALTER TABLE ""Users"" 
                DROP COLUMN IF EXISTS ""UserName"",
                DROP COLUMN IF EXISTS ""NormalizedUserName"",
                DROP COLUMN IF EXISTS ""NormalizedEmail"",
                DROP COLUMN IF EXISTS ""EmailConfirmed"",
                DROP COLUMN IF EXISTS ""SecurityStamp"",
                DROP COLUMN IF EXISTS ""ConcurrencyStamp"",
                DROP COLUMN IF EXISTS ""PhoneNumberConfirmed"",
                DROP COLUMN IF EXISTS ""TwoFactorEnabled"",
                DROP COLUMN IF EXISTS ""LockoutEnd"",
                DROP COLUMN IF EXISTS ""LockoutEnabled"",
                DROP COLUMN IF EXISTS ""AccessFailedCount"";
            ");

            // Update existing NULL values first, then set NOT NULL constraints
            migrationBuilder.Sql(@"
                -- Update existing NULL values BEFORE setting NOT NULL constraints
                UPDATE ""Users"" 
                SET 
                    ""PasswordHash"" = COALESCE(""PasswordHash"", ''),
                    ""FullName"" = COALESCE(""FullName"", COALESCE(""Email"", 'User')),
                    ""Role"" = COALESCE(""Role"", 0),
                    ""IsActive"" = COALESCE(""IsActive"", true),
                    ""EmailVerified"" = COALESCE(""EmailVerified"", false),
                    ""CreatedAt"" = COALESCE(""CreatedAt"", NOW()),
                    ""UpdatedAt"" = COALESCE(""UpdatedAt"", NOW())
                WHERE ""PasswordHash"" IS NULL OR ""FullName"" IS NULL OR ""PasswordHash"" = '' OR ""FullName"" = '' OR ""Email"" IS NULL;
            ");

            // Now set NOT NULL constraints
            migrationBuilder.Sql(@"
                -- Set NOT NULL constraints after updating NULL values
                ALTER TABLE ""Users""
                ALTER COLUMN ""Email"" SET NOT NULL,
                ALTER COLUMN ""PasswordHash"" SET NOT NULL,
                ALTER COLUMN ""FullName"" SET NOT NULL;
            ");

            // Create unique index on Email if not exists
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Email"" ON ""Users"" (""Email"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: Rollback is complex and would require recreating all Identity tables
            // This is intentionally left empty as rollback is not recommended
        }
    }
}
