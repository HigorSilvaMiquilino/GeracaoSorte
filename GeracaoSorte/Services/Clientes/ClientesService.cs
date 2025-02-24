using GeracaoSorte.Data;
using Microsoft.EntityFrameworkCore;

namespace GeracaoSorte.Services.Clientes
{
    public class ClientesService : IClientesService
    {
        private readonly ApplicationDbContext _context;
        public ClientesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClienteComNumeros>> GetClientesPorArquivoNome(string arquivoNome)
        {
            var clientes = await _context.Arquivos
                .Where(c => c.ArquivoNome == arquivoNome)
                .ToListAsync();

            if (clientes == null || !clientes.Any())
            {
                return null;
            }

            var resultados = new List<ClienteComNumeros>();

            foreach (var cliente in clientes)
            {
                var numerosGerados = GerarNumerosSorte(cliente.QtdNumSorteRegular, cliente.idCliente);

                await _context.ParticipacoesSorte.AddRangeAsync(numerosGerados);
                await _context.SaveChangesAsync();

                var numerosFormatados = numerosGerados.Select(n => $"{n.Serie}-{n.Ordem}").ToList();

                resultados.Add(new ClienteComNumeros
                {
                    IdCliente = cliente.idCliente,
                    QtdNumSorteRegular = cliente.QtdNumSorteRegular,
                    NumerosGerados = numerosGerados.Count,
                    Serie = numerosGerados.First().Serie, 
                    Ordem = numerosGerados.First().Ordem, 
                    NumerosDaSorte = string.Join(", ", numerosFormatados) 
                });
            }

            return resultados;
        }

        public async Task<List<string>> GetTodosArquivoNome()
        {
            return await _context.Arquivos
                .Where(c => c.ArquivoNome != null && c.ArquivoNome.Trim() != string.Empty)
                .Select(c => c.ArquivoNome)
                .Distinct()
                .ToListAsync();
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

                if (contagemPorSerie[serie] >= limiteSuperior)
                {
                    continue; 
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

