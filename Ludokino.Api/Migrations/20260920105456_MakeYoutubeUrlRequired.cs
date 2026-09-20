using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludokino.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeYoutubeUrlRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "Emissions"
                        WHERE "YoutubeUrl" IS NULL OR btrim("YoutubeUrl") = ''
                    ) THEN
                        RAISE EXCEPTION 'Impossible de rendre YoutubeUrl obligatoire : des emissions ont un lien manquant.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "YoutubeUrl",
                table: "Emissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "YoutubeUrl",
                table: "Emissions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
