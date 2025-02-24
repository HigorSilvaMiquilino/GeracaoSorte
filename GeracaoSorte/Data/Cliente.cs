namespace GeracaoSorte.Data
{
    public class Cliente
    {
        public int Id { get; set; }

        public ParticipacoesSorte Promocao { get; set; }

        public int? ParticipacoesSorteId { get; set; }

    }
}
