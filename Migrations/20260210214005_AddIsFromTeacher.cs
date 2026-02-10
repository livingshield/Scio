using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScioApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFromTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFromTeacher",
                table: "Scio_Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFromTeacher",
                table: "Scio_Messages");
        }
    }
}
