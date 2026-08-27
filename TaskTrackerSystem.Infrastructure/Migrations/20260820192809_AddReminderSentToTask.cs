using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTrackerSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderSentToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "Tasks");
        }
    }
}
