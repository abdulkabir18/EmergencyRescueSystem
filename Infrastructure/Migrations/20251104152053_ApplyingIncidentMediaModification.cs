using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApplyingIncidentMediaModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentMedias");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Incidents",
                type: "character varying(55)",
                maxLength: 55,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Medias",
                columns: table => new
                {
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileUrl = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MediaType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medias", x => new { x.IncidentId, x.Id });
                    table.ForeignKey(
                        name: "FK_Medias_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medias");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Incidents");

            migrationBuilder.CreateTable(
                name: "IncidentMedias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    MediaType = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_IncidentMedias_IncidentId_FileUrl",
                table: "IncidentMedias",
                columns: new[] { "IncidentId", "FileUrl" });
        }
    }
}
