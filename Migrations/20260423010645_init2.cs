using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace library.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_books_users_UserId",
                table: "books");

            migrationBuilder.DropIndex(
                name: "IX_books_UserId",
                table: "books");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "books");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "books",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_books_UserId",
                table: "books",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_books_users_UserId",
                table: "books",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}
