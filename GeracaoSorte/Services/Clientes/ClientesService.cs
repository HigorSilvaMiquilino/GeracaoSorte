using EFCore.BulkExtensions;
using GeracaoSorte.Data;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GeracaoSorte.Services.Clientes
{
    public class ClientesService : IClientesService
    {
        private readonly ApplicationDbContext _context;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
       


        public ClientesService(ApplicationDbContext context, DbContextOptions<ApplicationDbContext> dbContextOptions)
        {
            _context = context;
            _dbContextOptions = dbContextOptions;
        }

        public async Task<List<ClienteComNumeros>> GetClientesPorArquivoNome(string arquivoNome)
        {
            try
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
                        Status = "404",
                        StackTrace = "Nenhum cliente encontrado para o arquivo informado"
                    });
                    await _context.SaveChangesAsync();
                    return null;
                }

                var resultados = new List<ClienteComNumeros>();
                var todaParticipacoes = new List<ParticipacoesSorte>();
                var todosPares = new HashSet<string>();

                var stopwatch = Stopwatch.StartNew();

                foreach (var cliente in clientes)
                {
                    try
                    {
                        var numerosGerados = await GerarNumerosSorte(cliente.QtdNumSorteRegular, cliente.idCliente, todosPares);
                        todaParticipacoes.AddRange(numerosGerados);

                                foreach (var numero in numerosGerados)
                                {
                                    var numerosFormatados = numerosGerados
                                        .Where(n => n.Serie == numero.Serie && n.Ordem == numero.Ordem)
                                        .Select(n => $"{n.Serie}-{n.Ordem}")
                                        .ToList();

                                    resultados.Add(new ClienteComNumeros
                                    {
                                        IdCliente = cliente.idCliente,
                                        QtdNumSorteRegular = cliente.QtdNumSorteRegular,
                                        NumerosGerados = numerosGerados.Count,
                                        Serie = numero.Serie,
                                        Ordem = numero.Ordem,
                                        NumerosDaSorte = string.Join(", ", numerosFormatados)
                                    });
                                }                  
                    }
                    catch (Exception ex)
                    {
                        _context.LogsErro.Add(new LogErro
                        {
                            DataErro = DateTime.Now,
                            MensagemErro = $"Erro ao gerar números da sorte para o cliente {cliente.idCliente}: {ex.Message}",
                            StackTrace = ex.StackTrace,
                            Status = "Erro"
                        });
                        await _context.SaveChangesAsync();
                    }
                }

                stopwatch.Stop();
                long timeElapsed = stopwatch.ElapsedMilliseconds;

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        stopwatch.Restart();
                        await _context.BulkInsertAsync(todaParticipacoes);
                        await SalvarClientesComNumeros(resultados);
                        await transaction.CommitAsync();
                        stopwatch.Stop();
                        Console.WriteLine($"Tempo total para Gerar Números da sorte: {timeElapsed} ms");
                        Console.WriteLine($"Tempo total para salvar no banco: {stopwatch.ElapsedMilliseconds} ms");

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        var sqlException = ex.InnerException as SqlException;
                        _context.LogsErro.Add(new LogErro
                        {
                            DataErro = DateTime.Now,
                            MensagemErro = $"Erro ao salvar clientes com números:  {ex.Message}" +
                       (sqlException != null ? $" SQL Error: {sqlException.Number} - {sqlException.Message}" : ""),
                            StackTrace = ex.StackTrace,
                            Status = "Erro"
                        });
                        await _context.SaveChangesAsync();
                    }
                }

                return resultados.OrderBy(c => c.IdCliente).ThenBy(s => s.Serie).ToList();
            }
            catch (Exception ex)
            {
                _context.LogsErro.Add(new LogErro
                {
                    DataErro = DateTime.Now,
                    MensagemErro = ex.Message,
                    StackTrace = ex.StackTrace,
                    Status = ex.HResult.ToString()
                });
                await _context.SaveChangesAsync();
                return null;
            }
        }

        public async Task<List<string>> GetTodosArquivoNome()
        {
            return await _context.Arquivos
                .Where(c => c.ArquivoNome != null && c.ArquivoNome.Trim() != string.Empty)
                .Select(c => c.ArquivoNome)
                .Distinct()
                .ToListAsync();
        }


        public async Task<List<ParticipacoesSorte>> GerarNumerosSorte(int quantidade, int idCliente, HashSet<string> todosPares)
        {
            var participacoes = new List<ParticipacoesSorte>();
            var random = new Random();
            var contagemPorSerie = new ConcurrentDictionary<string, int>();

            for (int i = 0; i < 100; i++)
                contagemPorSerie[i.ToString("D2")] = 0;

            double mediaPorSerie = quantidade / 100.0;
            double limiteSuperior = mediaPorSerie * 1.05;


            try
            {
                    using (var context = new ApplicationDbContext(_dbContextOptions))
                    {
                        while (participacoes.Count < quantidade)
                        {
                            try
                            {
                            var candidato = new List<(string Serie, string Ordem)>();
                            for (int i = 0; i < Math.Min(1000, quantidade - participacoes.Count); i++)
                            {
                                var serie = random.Next(0, 100).ToString("D2");
                                var ordem = random.Next(0, 100000).ToString("D2");
                                if (contagemPorSerie[serie] < limiteSuperior)
                                {
                                    candidato.Add((serie, ordem));
                                }
                            }

                            var series = candidato.Select(c => c.Serie).Distinct().ToList();
                            var ordens = candidato.Select(c => c.Ordem).ToList();
                            var participacoesExistentes = await context.ParticipacoesSorte
                                .Where(p => series.Contains(p.Serie) && ordens.Contains(p.Ordem))
                                .Select(p => new { p.Serie, p.Ordem })
                                .ToListAsync();

                            var clientesExistentes = await context.ClienteComNumeros
                                .Where(c => series.Contains(c.Serie) && ordens.Contains(c.Ordem))
                                .Select(c => new { c.Serie, c.Ordem })
                                .ToListAsync();

                            var existeSet = participacoesExistentes.Concat(clientesExistentes)
                                .Select(p => $"{p.Serie}-{p.Ordem}")
                                .ToHashSet();

                            foreach (var (serie, ordem) in candidato)
                            {
                                if(!existeSet.Contains($"{serie}-{ordem}") && contagemPorSerie[serie] < limiteSuperior && !todosPares.Contains($"{serie}-{ordem}"))
                                { 
                                    participacoes.Add(new ParticipacoesSorte
                                    {
                                        Serie = serie,
                                        Ordem = ordem,
                                        DataCreate = DateTime.Now,
                                        Cliente = idCliente
                                    });
                                    contagemPorSerie.AddOrUpdate(serie, 1, (key, value) => value + 1); 
                                    todosPares.Add($"{serie}-{ordem}");
                                }
                            }

                        }
                            catch (Exception ex)
                            {
                                _context.LogsErro.Add(new LogErro
                                {
                                    DataErro = DateTime.Now,
                                    MensagemErro = ex.Message,
                                    StackTrace = ex.StackTrace,
                                    Status = ex.HResult.ToString()
                                });
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                return participacoes;
            }
            catch (Exception ex)
            {
                _context.LogsErro.Add(new LogErro
                {
                    DataErro = DateTime.Now,
                    MensagemErro = ex.Message,
                    StackTrace = ex.StackTrace,
                    Status = ex.HResult.ToString()
                });
                await _context.SaveChangesAsync();
            }

            return participacoes.ToList();
        }

        public async Task SalvarClientesComNumeros(List<ClienteComNumeros> clientes)
        {
            try
            {

                    try
                    {
                        await _context.ClienteComNumeros.AddRangeAsync(clientes);
                        await _context.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        _context.LogsErro.Add(new LogErro
                        {
                            DataErro = DateTime.Now,
                            MensagemErro = $"Erro ao salvar clientes com números: {ex.Message}",
                            StackTrace = ex.StackTrace,
                            Status = "Erro"
                        });
                        await _context.SaveChangesAsync();
                    }
                
            }
            catch (Exception ex)
            {
                _context.LogsErro.Add(new LogErro
                {
                    DataErro = DateTime.Now,
                    MensagemErro = $"Erro ao salvar clientes com números: {ex.Message}",
                    StackTrace = ex.StackTrace,
                    Status = "Erro"
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}