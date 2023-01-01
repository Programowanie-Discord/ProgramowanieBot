using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramowanieBot.Migrations;

/// <inheritdoc />
public partial class RenameReputationReactionsTodayToReputationToday : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ReputationReactionsToday",
            table: "Profiles",
            newName: "ReputationToday");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ReputationToday",
            table: "Profiles",
            newName: "ReputationReactionsToday");
    }
}
