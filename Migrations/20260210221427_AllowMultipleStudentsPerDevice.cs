using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScioApp.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleStudentsPerDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Scio_Students_GroupId_DeviceId",
                table: "Scio_Students");

            migrationBuilder.CreateIndex(
                name: "IX_Scio_Students_GroupId_DeviceId_Nickname",
                table: "Scio_Students",
                columns: new[] { "GroupId", "DeviceId", "Nickname" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Scio_Students_GroupId_DeviceId_Nickname",
                table: "Scio_Students");

            migrationBuilder.CreateIndex(
                name: "IX_Scio_Students_GroupId_DeviceId",
                table: "Scio_Students",
                columns: new[] { "GroupId", "DeviceId" },
                unique: true);
        }
    }
}
