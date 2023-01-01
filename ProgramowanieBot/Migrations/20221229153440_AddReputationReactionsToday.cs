using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramowanieBot.Migrations;

/// <inheritdoc />
public partial class AddReputationReactionsToday : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ReputationReactionsToday",
            table: "Profiles",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReputationReactionsToday",
            table: "Profiles");
    }
}
