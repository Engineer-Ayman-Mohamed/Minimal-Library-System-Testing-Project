using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class addDeleteRistrictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Books_BookId",
                schema: "Library",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Members_MemberId",
                schema: "Library",
                table: "Loans");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Books_BookId",
                schema: "Library",
                table: "Loans",
                column: "BookId",
                principalSchema: "Library",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Members_MemberId",
                schema: "Library",
                table: "Loans",
                column: "MemberId",
                principalSchema: "Library",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Books_BookId",
                schema: "Library",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Members_MemberId",
                schema: "Library",
                table: "Loans");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Books_BookId",
                schema: "Library",
                table: "Loans",
                column: "BookId",
                principalSchema: "Library",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Members_MemberId",
                schema: "Library",
                table: "Loans",
                column: "MemberId",
                principalSchema: "Library",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
