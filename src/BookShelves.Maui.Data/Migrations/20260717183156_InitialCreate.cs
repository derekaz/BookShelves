using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShelves.Maui.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatasyncDeltaTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatasyncDeltaTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatasyncOperationsQueue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAttempt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ItemId = table.Column<string>(type: "TEXT", maxLength: 126, nullable: false),
                    EntityVersion = table.Column<string>(type: "TEXT", maxLength: 126, nullable: false),
                    Item = table.Column<string>(type: "TEXT", nullable: false),
                    Sequence = table.Column<long>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatasyncOperationsQueue", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatasyncOperationsQueue_ItemId_EntityType",
                table: "DatasyncOperationsQueue",
                columns: new[] { "ItemId", "EntityType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorItems");

            migrationBuilder.DropTable(
                name: "DatasyncDeltaTokens");

            migrationBuilder.DropTable(
                name: "DatasyncOperationsQueue");
        }
    }
}
