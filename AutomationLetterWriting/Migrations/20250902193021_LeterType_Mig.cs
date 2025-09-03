using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationLetterWriting.Migrations
{
    /// <inheritdoc />
    public partial class LeterType_Mig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LetterTypeId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LetterTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LetterTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_LetterTypeId",
                table: "Messages",
                column: "LetterTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_LetterTypes_LetterTypeId",
                table: "Messages",
                column: "LetterTypeId",
                principalTable: "LetterTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_LetterTypes_LetterTypeId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "LetterTypes");

            migrationBuilder.DropIndex(
                name: "IX_Messages_LetterTypeId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "LetterTypeId",
                table: "Messages");
        }
    }
}
