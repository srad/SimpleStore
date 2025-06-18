using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleStore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedHosts",
                columns: table => new
                {
                    Hostname = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedHosts", x => x.Hostname);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AccessTimeLimited = table.Column<bool>(type: "INTEGER", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Buckets",
                columns: table => new
                {
                    BucketId = table.Column<string>(type: "TEXT COLLATE NOCASE", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "TEXT COLLATE NOCASE", nullable: false),
                    DirectoryName = table.Column<string>(type: "TEXT COLLATE NOCASE", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastAccess = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Private = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buckets", x => x.BucketId);
                });

            migrationBuilder.CreateTable(
                name: "BucketFiles",
                columns: table => new
                {
                    StorageFileId = table.Column<string>(type: "TEXT COLLATE NOCASE", maxLength: 36, nullable: false),
                    FileName = table.Column<string>(type: "TEXT COLLATE NOCASE", maxLength: 1024, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT COLLATE NOCASE", maxLength: 1024, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT COLLATE NOCASE", maxLength: 2048, nullable: false),
                    Url = table.Column<string>(type: "TEXT COLLATE NOCASE", maxLength: 2048, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    FileSizeMB = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AccessCount = table.Column<long>(type: "INTEGER", nullable: false),
                    Private = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastAccess = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    BucketId = table.Column<string>(type: "TEXT COLLATE NOCASE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BucketFiles", x => x.StorageFileId);
                    table.ForeignKey(
                        name: "FK_BucketFiles_Buckets_BucketId",
                        column: x => x.BucketId,
                        principalTable: "Buckets",
                        principalColumn: "BucketId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BucketFiles_BucketId",
                table: "BucketFiles",
                column: "BucketId");

            migrationBuilder.CreateIndex(
                name: "IX_Buckets_DirectoryName",
                table: "Buckets",
                column: "DirectoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buckets_Name",
                table: "Buckets",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedHosts");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "BucketFiles");

            migrationBuilder.DropTable(
                name: "Buckets");
        }
    }
}
