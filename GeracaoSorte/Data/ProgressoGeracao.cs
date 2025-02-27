namespace GeracaoSorte.Data
{
    public class ProgressoGeracao
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public int QuantidadeGerada { get; set; }
        public DateTime UltimaAtualizacao { get; set; }
    }
}
