using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeracaoSorte.Migrations
{
    /// <inheritdoc />
    public partial class ClientesComNumeros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.CreateTable(
                name: "ClienteComNumeros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    QtdNumSorteRegular = table.Column<int>(type: "int", nullable: false),
                    NumerosGerados = table.Column<int>(type: "int", nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumerosDaSorte = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteComNumeros", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClienteComNumeros");

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipacoesSorteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_ParticipacoesSorte_ParticipacoesSorteId",
                        column: x => x.ParticipacoesSorteId,
                        principalTable: "ParticipacoesSorte",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ParticipacoesSorteId",
                table: "Clientes",
                column: "ParticipacoesSorteId");
        }
    }
}
