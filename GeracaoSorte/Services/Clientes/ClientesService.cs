using GeracaoSorte.Data;
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

                foreach (var cliente in clientes)
                {
                    try
                    {
                        var numerosGerados = await GerarNumerosSorte(cliente.QtdNumSorteRegular, cliente.idCliente);

                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {

                                await _context.ParticipacoesSorte.AddRangeAsync(numerosGerados);
                                await _context.SaveChangesAsync();


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

                                await transaction.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
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
                resultados = resultados.
                    OrderBy(c => c.IdCliente)
                    .ThenBy(s => s.Serie)
                    .ToList();

                await SalvarClientesComNumeros(resultados);

                return resultados;
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


        public async Task<List<ParticipacoesSorte>> GerarNumerosSorte(int quantidade, int idCliente)
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

<<<<<<< HEAD
   
=======
            int quantidadeGerada = await RecuperarProgresso(idCliente);
>>>>>>> 37d7bf77bdde47fac2d27906ce3256958304b7bd


            try
            {

<<<<<<< HEAD
                await Parallel.ForEachAsync(Enumerable.Range(0,quantidade), async (i, cancellationToken) =>
=======
                await Parallel.ForEachAsync(Enumerable.Range(quantidadeGerada, quantidade - quantidadeGerada), async (i, cancellationToken) =>
>>>>>>> 37d7bf77bdde47fac2d27906ce3256958304b7bd
                {
                    using (var context = new ApplicationDbContext(_dbContextOptions))
                    {
                        while (true)
                        {
                            try
                            {
                                string serie = random.Next(0, 100).ToString("D2");
                                string ordem = random.Next(0, 100000).ToString("D5");
                                string numeroSorte = $"{serie}-{ordem}";

                                if (numerosGerados.Contains(numeroSorte))
                                {
                                    continue;
                                }

                                bool numeroExisteNoBanco = await context.ParticipacoesSorte
                                    .AnyAsync(p => p.Serie == serie && p.Ordem == ordem, cancellationToken);

                                if (numeroExisteNoBanco)
                                {
                                    continue;
                                }

                                bool clienteJaTemNumero = await context.ClienteComNumeros
                                    .AnyAsync(c => c.Serie == serie && c.Ordem == ordem, cancellationToken);

                                if (clienteJaTemNumero)
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

<<<<<<< HEAD

=======
                                quantidadeGerada++;
                                if (quantidadeGerada % 1000 == 0)
                                {
                                    await SalvarProgresso(idCliente, quantidadeGerada);
                                }
>>>>>>> 37d7bf77bdde47fac2d27906ce3256958304b7bd

                                break;
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
                });
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
            stopwatch.Stop();
            await SalvarProgresso(idCliente, quantidadeGerada);
            return participacoes.ToList();
        }
        public async Task SalvarClientesComNumeros(List<ClienteComNumeros> clientes)
        {
            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.ClienteComNumeros.AddRangeAsync(clientes);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
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
<<<<<<< HEAD
=======

        public async Task SalvarProgresso(int idCliente, int quantidadeGerada)
        {
            var progresso = await _context.ProgressoGeracoes
                .FirstOrDefaultAsync(p => p.IdCliente == idCliente);

            if (progresso == null)
            {
                progresso = new ProgressoGeracao
                {
                    IdCliente = idCliente,
                    QuantidadeGerada = quantidadeGerada,
                    UltimaAtualizacao = DateTime.Now
                };
                _context.ProgressoGeracoes.Add(progresso);
            }
            else
            {
                progresso.QuantidadeGerada = quantidadeGerada;
                progresso.UltimaAtualizacao = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> RecuperarProgresso(int idCliente)
        {
            var progresso = await _context.ProgressoGeracoes
                .FirstOrDefaultAsync(p => p.IdCliente == idCliente);

            return progresso?.QuantidadeGerada ?? 0;
        }
>>>>>>> 37d7bf77bdde47fac2d27906ce3256958304b7bd
    }
}