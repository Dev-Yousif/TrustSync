using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderIsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "Reminders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "Reminders");
        }
    }
}
