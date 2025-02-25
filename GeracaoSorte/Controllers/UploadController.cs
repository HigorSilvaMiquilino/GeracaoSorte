using CsvHelper;
using GeracaoSorte.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace GeracaoSorte.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UploadController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("UploadCsv")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _context.LogsErro.Add(new LogErro
                {
                    DataErro = DateTime.Now,
                    MensagemErro = "Nenhum arquivo enviado.",
                    StackTrace = "",
                    Status = "404",
                });
                return BadRequest(new { success = false, message = "Nenhum arquivo enviado." });
            }

            try
            {
                var arquivos = new List<Arquivo>();
                var fileName = file.FileName;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    if (file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var bufferedStream = new BufferedStream(stream))
                        using (var reader = new StreamReader(bufferedStream))
                        {
                         
                            var primeiraLinha = await reader.ReadLineAsync();
                            if (primeiraLinha == null)
                            {
                                _context.LogsErro.Add(new LogErro
                                {
                                    DataErro = DateTime.Now,
                                    MensagemErro = "O arquivo CSV está vazio ou não contém cabeçalhos.",
                                    StackTrace = "",
                                    Status = "400",
                                });
                                return BadRequest(new { success = false, message = "O arquivo CSV está vazio ou não contém cabeçalhos." });
                            }

                            var colunasDeFato = primeiraLinha.Split(','); 

                            var colunasEsperadas = new[] { "idCliente", "QtdNumSorteRegular" };

                            if (colunasDeFato == null || !colunasEsperadas.SequenceEqual(colunasDeFato))
                            {
                                _context.LogsErro.Add(new LogErro
                                {
                                    DataErro = DateTime.Now,
                                    MensagemErro = "Os títulos das colunas não correspondem ao esperado.",
                                    StackTrace = "",
                                    Status = "400",
                                });
                                return BadRequest(new { success = false, message = "Os títulos das colunas não correspondem ao esperado." });
                            }

                            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                            {
                                int row = 1; 
                                var erros = new List<string>();

                                while (csv.Read())
                                {
                                    row++;

                                    var idClienteStr = csv.GetField(0); 
                                    var qtdNumSorteRegularStr = csv.GetField(1); 

                                    if (!int.TryParse(idClienteStr, out int idCliente))
                                    {
                                        erros.Add($"O campo 'idCliente' na linha {row} não é um número válido.");
                                        continue; 
                                    }

                                    if (!int.TryParse(qtdNumSorteRegularStr, out int qtdNumSorteRegular))
                                    {
                                        erros.Add($"O campo 'QtdNumSorteRegular' na linha {row} não é um número válido.");
                                        continue; 
                                    }

                                    if (qtdNumSorteRegular <= 0)
                                    {
                                        erros.Add($"O campo 'QtdNumSorteRegular' na linha {row} deve ser maior que zero.");
                                        continue; 
                                    }

                                    arquivos.Add(new Arquivo
                                    {
                                        idCliente = idCliente,
                                        QtdNumSorteRegular = qtdNumSorteRegular,
                                        ArquivoNome = fileName
                                    });
                                }

                                if (erros.Any())
                                {
                                    foreach (var erro in erros)
                                    {
                                        _context.LogsErro.Add(new LogErro
                                        {
                                            DataErro = DateTime.Now,
                                            MensagemErro = erro,
                                            StackTrace = "",
                                            Status = "400",
                                        });
                                    }

                                    await _context.SaveChangesAsync(); 

                                    return BadRequest(new
                                    {
                                        success = false,
                                        message = "Erros encontrados durante o processamento do arquivo.",
                                        erros = erros
                                    });
                                }
                            }
                        }
                    }
                    else if (file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension?.Rows ?? 0;

                            if (rowCount == 0)
                            {
                                _context.LogsErro.Add(new LogErro
                                {
                                    DataErro = DateTime.Now,
                                    MensagemErro = "O arquivo Excel está vazio ou não contém dados.",
                                    StackTrace = "",
                                    Status = "400"
                                });

                                await _context.SaveChangesAsync();

                                return BadRequest(new { success = false, message = "O arquivo Excel está vazio ou não contém dados." });
                            }

                            var colunasEsperadas = new[] { "idCliente", "QtdNumSorteRegular" };
                            var colunasDeFato = new List<string>();

                            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                            {
                                var nomeColuna = worksheet.Cells[1, col].Text;
                                if (string.IsNullOrEmpty(nomeColuna))
                                {
                                    break;
                                }
                                colunasDeFato.Add(nomeColuna);
                            }

                            if (!colunasEsperadas.SequenceEqual(colunasDeFato))
                            {
                                _context.LogsErro.Add(new LogErro
                                {
                                    DataErro = DateTime.Now,
                                    MensagemErro = "Os títulos das colunas não correspondem ao esperado.",
                                    StackTrace = "",
                                    Status = "400"
                                });

                                await _context.SaveChangesAsync();

                                return BadRequest(new { success = false, message = "Os títulos das colunas não correspondem ao esperado." });
                            }

                            var linhas = new List<(string idClienteStr, string qtdNumSorteRegularStr, int row)>();

                            for (int row = 2; row <= rowCount; row++)
                            {
                                var idClientStr = worksheet.Cells[row, 1].Text;
                                var qtdNumSorteRegularStr = worksheet.Cells[row, 2].Text;

                                if (string.IsNullOrWhiteSpace(idClientStr) && string.IsNullOrEmpty(qtdNumSorteRegularStr))
                                {
                                    continue;
                                }

                                linhas.Add((idClientStr, qtdNumSorteRegularStr, row));
                            }

                            var arquivosConcorrentes = new ConcurrentBag<Arquivo>();
                            var errosConcorrentes = new ConcurrentBag<(string MensagemErro, int Linha)>();

                            Parallel.ForEach(linhas, (linha) =>
                            {
                                if (string.IsNullOrWhiteSpace(linha.idClienteStr) || string.IsNullOrWhiteSpace(linha.qtdNumSorteRegularStr))
                                {
                                    return; 
                                }

                                if (!int.TryParse(linha.idClienteStr, out int idCliente))
                                {
                                    errosConcorrentes.Add(($"O campo 'idCliente' na linha {linha.row} não é um número válido.", linha.row));
                                    return; 
                                }

                                if (!int.TryParse(linha.qtdNumSorteRegularStr, out int qtdNumSorteRegular))
                                {
                                    errosConcorrentes.Add(($"O campo 'QtdNumSorteRegular' na linha {linha.row} não é um número válido.", linha.row));
                                    return; 
                                }

                                if (qtdNumSorteRegular <= 0)
                                {
                                    errosConcorrentes.Add(($"O campo 'QtdNumSorteRegular' na linha {linha.row} deve ser maior que zero.", linha.row));
                                    return; 
                                }

                                arquivosConcorrentes.Add(new Arquivo
                                {
                                    idCliente = idCliente,
                                    QtdNumSorteRegular = qtdNumSorteRegular,
                                    ArquivoNome = fileName
                                });
                            });

                            if (errosConcorrentes.Any())
                            {
                                foreach (var erro in errosConcorrentes)
                                {
                                    _context.LogsErro.Add(new LogErro
                                    {
                                        DataErro = DateTime.Now,
                                        MensagemErro = erro.MensagemErro,
                                        StackTrace = "",
                                        Status = "400"
                                    });
                                }

                                await _context.SaveChangesAsync(); 

                                return BadRequest(new
                                {
                                    success = false,
                                    message = "Erros encontrados durante o processamento do arquivo.",
                                    erros = errosConcorrentes.Select(e => new { e.MensagemErro, e.Linha }).ToList()
                                });
                            }

                            await _context.Arquivos.AddRangeAsync(arquivosConcorrentes.ToList());
                            

                            _context.LogsSucesso.Add(new LogSucesso
                            {
                                Data = DateTime.Now,
                                Mensagem = "Arquivo processado com sucesso." + file.Name,
                                Status = "200"
                            });

                            await _context.SaveChangesAsync();

                            return Ok(new { success = true, message = "Arquivo processado com sucesso!" });
                        }
                    }
                    else
                    {
                        _context.LogsErro.Add(new LogErro
                        {
                            DataErro = DateTime.Now,
                            MensagemErro = "Formato de arquivo não suportado.",
                            StackTrace = "",
                            Status = "400"
                        });
                        await _context.SaveChangesAsync();

                        return BadRequest(new { success = false, message = "Formato de arquivo não suportado." });
                    }
                }
            }
            catch (Exception ex)
            {
                _context.LogsErro.Add(new LogErro
                {
                    DataErro = DateTime.Now,
                    MensagemErro = $"Erro interno no servidor: {ex.Message}",
                    StackTrace = ex.StackTrace,
                    Status = "500"
                });

                await _context.SaveChangesAsync();

                return StatusCode(500, new { success = false, message = $"Erro interno no servidor: {ex.Message}" });
            }
            _context.LogsSucesso.Add(new LogSucesso
            {
                Data = DateTime.Now,
                Mensagem = "Arquivo processado com sucesso." + file.Name,
                Status = "200"
            });

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Arquivo processado com sucesso!" });
        }

        [HttpPost("UploadSql")]
        public async Task<IActionResult> UploadSql(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "Nenhum arquivo enviado." });
            }

            var arquivos = new List<Arquivo>();

            try
            {
                var fileName = file.FileName;
                var sqlCommands = new List<string>();

                using (var stream = new StreamReader(file.OpenReadStream()))
                {
                    string line;
                    while ((line = await stream.ReadLineAsync()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("--"))
                        {
                            sqlCommands.Add(line);
                        }
                    }
                }

                var clientes = new List<Cliente>();

                foreach (var command in sqlCommands)
                {
                    var match = Regex.Match(command, @"INSERT INTO Arquivo \(idCliente, QtdNumSorteRegular, ArquivoNome\) VALUES \('(?<idCliente>[^']*)', '(?<QtdNumSorteRegular>[^']*)', (?<ArquivoNome>\d+)\)");
                    if (match.Success)
                    {
                        var idClienteStr = match.Groups["idCliente"].Value;
                        var qtdNumSorteRegularStr = match.Groups["QtdNumSorteRegular"].Value;


                        if (!int.TryParse(idClienteStr, out int idCliente))
                        {
                            return BadRequest(new { success = false, message = $"O campo 'idCliente' não é um número válido." });
                        }

                        if (!int.TryParse(qtdNumSorteRegularStr, out int qtdNumSorteRegular))
                        {
                            return BadRequest(new { success = false, message = $"O campo 'QtdNumSorteRegular' não é um número válido." });
                        }

                        if (qtdNumSorteRegular <= 0)
                        {
                            return BadRequest(new { success = false, message = $"O campo 'QtdNumSorteRegular' deve ser maior que zero." });
                        }

                        arquivos.Add(new Arquivo
                        {
                            idCliente = idCliente,
                            QtdNumSorteRegular = qtdNumSorteRegular,
                            ArquivoNome = fileName
                        });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = $"Comando SQL inválido: {command}." });
                    }
                }

                await _context.Clientes.AddRangeAsync(clientes);
                await _context.SaveChangesAsync();

                _context.LogsSucesso.Add(new LogSucesso
                {
                    Data = DateTime.Now,
                    Mensagem = "Arquivo processado com sucesso.",
                    Status = "200"
                });

                return Ok(new { success = true, message = "Arquivo SQL processado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Erro interno no servidor: {ex.Message}" });
            }
        }
    }
}
