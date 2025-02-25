using GeracaoSorte.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;

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
                _context.LogsErro.Add(new LogErro
                {
                    DataErro = DateTime.Now,
                    MensagemErro = "Nenhum cliente encontrado para o arquivo informado",
                });
                await _context.SaveChangesAsync();  
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
            var participacoes = new ConcurrentBag<ParticipacoesSorte>();
            var numerosGerados = new ConcurrentBag<string>();
            var contagemPorSerie = new ConcurrentDictionary<string, int>();

            for (int i = 0; i < 100; i++)
            {
                contagemPorSerie[i.ToString("D2")] = 0;
            }

            double mediaPorSerie = quantidade / 100.0;
            double limiteSuperior = mediaPorSerie * 1.05;

            var random = new Random();
            var stopwatch = Stopwatch.StartNew();

            Parallel.For(0, quantidade, i =>
            {
                while (true)
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
                    contagemPorSerie.AddOrUpdate(serie, 1, (key, oldValue) => oldValue + 1);

                    break;
                }
            });
            stopwatch.Stop();
            return participacoes.ToList();
        }
    }
}

