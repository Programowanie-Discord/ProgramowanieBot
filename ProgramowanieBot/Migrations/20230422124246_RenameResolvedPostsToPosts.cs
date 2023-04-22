using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramowanieBot.Migrations;

/// <inheritdoc />
public partial class RenameResolvedPostsToPosts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(
            name: "ResolvedPosts",
            newName: "Posts");
        migrationBuilder.RenameColumn(
            name: "Id",
            table: "Posts",
            newName: "PostId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "PostId",
            table: "Posts",
            newName: "Id");
        migrationBuilder.RenameTable(
            name: "Posts",
            newName: "ResolvedPosts");
    }
}
