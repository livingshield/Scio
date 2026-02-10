using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScioApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Scio_Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Login = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GoogleId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scio_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scio_Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GoalDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    GoalType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    InviteCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scio_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scio_Groups_Scio_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Scio_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scio_Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scio_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scio_Students_Scio_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Scio_Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scio_Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsSystemMessage = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsProgressContribution = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AIConfidence = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scio_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scio_Messages_Scio_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Scio_Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scio_Messages_Scio_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Scio_Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Scio_ProgressLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CurrentValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scio_ProgressLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scio_ProgressLogs_Scio_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Scio_Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scio_Groups_InviteCode",
                table: "Scio_Groups",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scio_Groups_TeacherId",
                table: "Scio_Groups",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Scio_Messages_GroupId_Timestamp",
                table: "Scio_Messages",
                columns: new[] { "GroupId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Scio_Messages_StudentId",
                table: "Scio_Messages",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Scio_ProgressLogs_StudentId",
                table: "Scio_ProgressLogs",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scio_Students_GroupId_DeviceId",
                table: "Scio_Students",
                columns: new[] { "GroupId", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scio_Users_GoogleId",
                table: "Scio_Users",
                column: "GoogleId",
                unique: true,
                filter: "[GoogleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Scio_Users_Login",
                table: "Scio_Users",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Scio_Messages");

            migrationBuilder.DropTable(
                name: "Scio_ProgressLogs");

            migrationBuilder.DropTable(
                name: "Scio_Students");

            migrationBuilder.DropTable(
                name: "Scio_Groups");

            migrationBuilder.DropTable(
                name: "Scio_Users");
        }
    }
}
