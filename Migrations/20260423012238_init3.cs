using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace library.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_loanBooks",
                table: "loanBooks");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "loanBooks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_loanBooks",
                table: "loanBooks",
                columns: new[] { "LoanId", "BookId" });

            migrationBuilder.CreateIndex(
                name: "IX_loanBooks_BookId",
                table: "loanBooks",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_loanBooks_books_BookId",
                table: "loanBooks",
                column: "BookId",
                principalTable: "books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_loanBooks_loans_LoanId",
                table: "loanBooks",
                column: "LoanId",
                principalTable: "loans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loanBooks_books_BookId",
                table: "loanBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_loanBooks_loans_LoanId",
                table: "loanBooks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_loanBooks",
                table: "loanBooks");

            migrationBuilder.DropIndex(
                name: "IX_loanBooks_BookId",
                table: "loanBooks");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "loanBooks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_loanBooks",
                table: "loanBooks",
                column: "Id");
        }
    }
}
