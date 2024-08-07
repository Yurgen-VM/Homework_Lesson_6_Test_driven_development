using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_4.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_New : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reseived",
                table: "Messages",
                newName: "Received");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Received",
                table: "Messages",
                newName: "Reseived");
        }
    }
}
