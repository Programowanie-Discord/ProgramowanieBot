using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramowanieBot.Migrations;

/// <inheritdoc />
public partial class AddResolvedPosts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ResolvedPosts",
            columns: table => new
            {
                Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ResolvedPosts", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ResolvedPosts");
    }
}
