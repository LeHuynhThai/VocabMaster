using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabMaster.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAvatarUrlFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa ràng buộc mặc định trước khi xóa cột
            migrationBuilder.Sql(@"
                DECLARE @constraintName nvarchar(200)
                
                SELECT @constraintName = name
                FROM sys.default_constraints
                WHERE parent_object_id = OBJECT_ID('Users')
                AND col_name(parent_object_id, parent_column_id) = 'AvatarUrl';
                
                IF @constraintName IS NOT NULL
                    EXEC('ALTER TABLE [Users] DROP CONSTRAINT ' + @constraintName)
                
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = 'AvatarUrl')
                    ALTER TABLE [Users] DROP COLUMN [AvatarUrl]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = 'AvatarUrl') BEGIN ALTER TABLE [Users] ADD [AvatarUrl] NVARCHAR(255) NULL END");
        }
    }
}
