using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoundsApp.Migrations
{
    /// <inheritdoc />
    public partial class FriendshipStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First convert string values to integers
            migrationBuilder.Sql(@"
                ALTER TABLE ""Friendships""
                ALTER COLUMN ""Status"" TYPE integer
                USING CASE
                    WHEN ""Status"" = 'pending' THEN 0
                    WHEN ""Status"" = 'Pending' THEN 0
                    WHEN ""Status"" = 'accepted' THEN 1
                    WHEN ""Status"" = 'Accepted' THEN 1
                    WHEN ""Status"" = 'rejected' THEN 2
                    WHEN ""Status"" = 'Rejected' THEN 2
                    ELSE 0
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Friendships",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
