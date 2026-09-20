using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludokino.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddYoutubeSyncMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Emissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LatestVideoId",
                table: "Emissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YoutubePlaylistId",
                table: "Emissions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Emissions");

            migrationBuilder.DropColumn(
                name: "LatestVideoId",
                table: "Emissions");

            migrationBuilder.DropColumn(
                name: "YoutubePlaylistId",
                table: "Emissions");
        }
    }
}
