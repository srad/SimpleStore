using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleStore.Migrations
{
    /// <inheritdoc />
    public partial class FilesAsDownload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AsDownload",
                table: "Buckets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AsDownload",
                table: "BucketFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsDownload",
                table: "Buckets");

            migrationBuilder.DropColumn(
                name: "AsDownload",
                table: "BucketFiles");
        }
    }
}
