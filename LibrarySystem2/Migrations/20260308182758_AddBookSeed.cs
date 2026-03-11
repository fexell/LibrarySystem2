using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library2.Migrations
{
    /// <inheritdoc />
    public partial class AddBookSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Members_MemberId1",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_MemberId1",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "MemberId1",
                table: "Loans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberId1",
                table: "Loans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_MemberId1",
                table: "Loans",
                column: "MemberId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Members_MemberId1",
                table: "Loans",
                column: "MemberId1",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
