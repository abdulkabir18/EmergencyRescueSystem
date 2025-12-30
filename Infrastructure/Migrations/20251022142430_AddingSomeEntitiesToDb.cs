using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingSomeEntitiesToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupportedIncidents",
                table: "Agencies",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_LGA = table.Column<string>(type: "text", nullable: true),
                    Address_Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentMedias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MediaType = table.Column<int>(type: "integer", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentMedias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentMedias_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentResponders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentResponders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentResponders_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncidentResponders_Responders_ResponderId",
                        column: x => x.ResponderId,
                        principalTable: "Responders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentMedias_IncidentId_FileUrl",
                table: "IncidentMedias",
                columns: new[] { "IncidentId", "FileUrl" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentResponders_IncidentId_ResponderId",
                table: "IncidentResponders",
                columns: new[] { "IncidentId", "ResponderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentResponders_ResponderId",
                table: "IncidentResponders",
                column: "ResponderId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ReferenceCode",
                table: "Incidents",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_UserId",
                table: "Incidents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentMedias");

            migrationBuilder.DropTable(
                name: "IncidentResponders");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropColumn(
                name: "SupportedIncidents",
                table: "Agencies");
        }
    }
}
