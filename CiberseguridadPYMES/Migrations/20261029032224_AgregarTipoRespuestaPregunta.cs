using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CiberseguridadPYMES.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoRespuestaPregunta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoRespuesta",
                table: "Preguntas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoRespuesta",
                table: "Preguntas");
        }
    }
}
