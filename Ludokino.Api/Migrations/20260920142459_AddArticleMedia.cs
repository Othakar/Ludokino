using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludokino.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "Articles",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Articles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Articles");
        }
    }
}
