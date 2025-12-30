using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitailCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "character varying(555)", maxLength: 555, nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_LGA = table.Column<string>(type: "text", nullable: true),
                    Address_Country = table.Column<string>(type: "text", nullable: true),
                    Address_PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_LGA = table.Column<string>(type: "text", nullable: true),
                    Address_Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AgencyAdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agencies_Users_AgencyAdminId",
                        column: x => x.AgencyAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Responders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedLatitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: true),
                    AssignedLongitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Responders_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Responders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_AgencyAdminId",
                table: "Agencies",
                column: "AgencyAdminId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Responders_AgencyId",
                table: "Responders",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Responders_UserId",
                table: "Responders",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Responders");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
