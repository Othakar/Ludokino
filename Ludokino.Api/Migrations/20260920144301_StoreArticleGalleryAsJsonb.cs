using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ludokino.Api.Migrations
{
    /// <inheritdoc />
    public partial class StoreArticleGalleryAsJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Articles\" ALTER COLUMN \"ImageUrls\" DROP DEFAULT;");
            migrationBuilder.Sql("""
                ALTER TABLE "Articles"
                ALTER COLUMN "ImageUrls" TYPE jsonb
                USING CASE
                    WHEN NULLIF("ImageUrls", '') IS NULL THEN '[]'::jsonb
                    ELSE "ImageUrls"::jsonb
                END;
                """);
            migrationBuilder.Sql("ALTER TABLE \"Articles\" ALTER COLUMN \"ImageUrls\" SET DEFAULT '[]'::jsonb;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Articles\" ALTER COLUMN \"ImageUrls\" DROP DEFAULT;");
            migrationBuilder.Sql("""
                ALTER TABLE "Articles"
                ALTER COLUMN "ImageUrls" TYPE text
                USING "ImageUrls"::text;
                """);
            migrationBuilder.Sql("ALTER TABLE \"Articles\" ALTER COLUMN \"ImageUrls\" SET DEFAULT '[]';");
        }
    }
}
