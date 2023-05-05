using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramowanieBot.Migrations;

/// <inheritdoc />
public partial class AddPostResolveReminderCounter : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsResolved",
            table: "Posts",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<int>(
            name: "PostResolveReminderCounter",
            table: "Posts",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsResolved",
            table: "Posts");

        migrationBuilder.DropColumn(
            name: "PostResolveReminderCounter",
            table: "Posts");
    }
}
