using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShelves.Maui.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookModelRenamePublishDate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublisherDate",
                table: "BookItems",
                newName: "PublishDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublishDate",
                table: "BookItems",
                newName: "PublisherDate");
        }
    }
}
