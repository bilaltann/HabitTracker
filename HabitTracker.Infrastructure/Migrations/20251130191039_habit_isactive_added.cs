using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class habit_isactive_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Habits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Habits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompletedToday",
                table: "HabitLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "IsCompletedToday",
                table: "HabitLogs");
        }
    }
}
