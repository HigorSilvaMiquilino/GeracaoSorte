using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeracaoSorte.Migrations
{
    /// <inheritdoc />
    public partial class Indices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Serie",
                table: "ParticipacoesSorte",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Ordem",
                table: "ParticipacoesSorte",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Serie",
                table: "ClienteComNumeros",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Ordem",
                table: "ClienteComNumeros",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesSorte_Ordem",
                table: "ParticipacoesSorte",
                column: "Ordem");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesSorte_Serie",
                table: "ParticipacoesSorte",
                column: "Serie");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesSorte_Serie_Ordem",
                table: "ParticipacoesSorte",
                columns: new[] { "Serie", "Ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClienteComNumeros_Ordem",
                table: "ClienteComNumeros",
                column: "Ordem");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteComNumeros_Serie",
                table: "ClienteComNumeros",
                column: "Serie");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteComNumeros_Serie_Ordem",
                table: "ClienteComNumeros",
                columns: new[] { "Serie", "Ordem" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParticipacoesSorte_Ordem",
                table: "ParticipacoesSorte");

            migrationBuilder.DropIndex(
                name: "IX_ParticipacoesSorte_Serie",
                table: "ParticipacoesSorte");

            migrationBuilder.DropIndex(
                name: "IX_ParticipacoesSorte_Serie_Ordem",
                table: "ParticipacoesSorte");

            migrationBuilder.DropIndex(
                name: "IX_ClienteComNumeros_Ordem",
                table: "ClienteComNumeros");

            migrationBuilder.DropIndex(
                name: "IX_ClienteComNumeros_Serie",
                table: "ClienteComNumeros");

            migrationBuilder.DropIndex(
                name: "IX_ClienteComNumeros_Serie_Ordem",
                table: "ClienteComNumeros");

            migrationBuilder.AlterColumn<string>(
                name: "Serie",
                table: "ParticipacoesSorte",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Ordem",
                table: "ParticipacoesSorte",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Serie",
                table: "ClienteComNumeros",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Ordem",
                table: "ClienteComNumeros",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
