using GeracaoSorte.Data;

namespace GeracaoSorte.Services.Sorte
{
    public class GeracaoSorte
    {
        private readonly ApplicationDbContext _context;

        public GeracaoSorte(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ParticipacoesSorte> GerarNumerosSorte(int quantidade, int idCliente)
        {
            List<ParticipacoesSorte> participacoes = new List<ParticipacoesSorte>();
            Random random = new Random();
            HashSet<string> numerosGerados = new HashSet<string>();
            Dictionary<string, int> contagemPorSerie = new Dictionary<string, int>();

            for (int i = 0; i < 100; i++)
            {
                contagemPorSerie[i.ToString("D2")] = 0;
            }

            // Calcula a média esperada de números por série
            double mediaPorSerie = quantidade / 100.0;
            double limiteSuperior = mediaPorSerie * 1.05; 

            while (participacoes.Count < quantidade)
            {
                string serie = random.Next(0, 100).ToString("D2"); 
                string ordem = random.Next(0, 100000).ToString("D5");
                string numeroSorte = $"{serie}-{ordem}";

                if (numerosGerados.Contains(numeroSorte))
                {
                    continue;
                }

                // Verifica o balanceamento entre séries
                if (contagemPorSerie[serie] >= limiteSuperior)
                {
                    continue; // Série já atingiu o limite, gera outra série
                }


                participacoes.Add(new ParticipacoesSorte
                {
                    Serie = serie,
                    Ordem = ordem,
                    DataCreate = DateTime.Now,
                    Cliente = idCliente 
                });

                numerosGerados.Add(numeroSorte);
                contagemPorSerie[serie]++;
            }
            return participacoes;
        }
    }    
}

