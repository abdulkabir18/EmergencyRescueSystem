using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrectionForMediaAtIncident : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medias");

            migrationBuilder.AddColumn<string>(
                name: "Media_FileUrl",
                table: "Incidents",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Media_MediaType",
                table: "Incidents",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Media_FileUrl",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Media_MediaType",
                table: "Incidents");

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
    }
}
