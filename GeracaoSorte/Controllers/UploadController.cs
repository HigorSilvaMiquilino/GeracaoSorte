using CsvHelper;
using GeracaoSorte.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Globalization;
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
                        using (var reader = new StreamReader(stream))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            csv.Read();
                            csv.ReadHeader();

                            var colunasEsperadas = new[] { "idCliente", "QtdNumSorteRegular" };
                            var colunasDeFato = csv.HeaderRecord;

                            var colunasDeFatoArray = string.Join(", ", colunasDeFato);

                            colunasDeFato = colunasDeFatoArray.Split(';');

                            string[] colunas = new string[2];
                            foreach (var coluna in colunasDeFato)
                            {
                                if (coluna == "idCliente")
                                {
                                    colunas[0] = coluna;
                                }
                                else if (coluna == "QtdNumSorteRegular")
                                {
                                    colunas[1] = coluna;
                                }
 
                            }

                            if (colunasDeFato == null || !colunasEsperadas.SequenceEqual(colunas))
                            {
                                return BadRequest(new { success = false, message = "Os títulos das colunas não correspondem ao esperado." });
                            }

                            int row = 1;
                            while (csv.Read())
                            {
                                row++;

                                var idClienteStr = csv.GetField("idCliente");
                                var qtdNumSorteRegularStr = csv.GetField("QtdNumSorteRegular");


                                if (!int.TryParse(idClienteStr, out int idCliente))
                                {
                                    return BadRequest(new { success = false, message = $"O campo 'idCliente' na linha {row} não é um número válido." });
                                }

                                if (!int.TryParse(qtdNumSorteRegularStr, out int qtdNumSorteRegular))
                                {
                                    return BadRequest(new { success = false, message = $"O campo 'QtdNumSorteRegular' na linha {row} não é um número válido." });
                                }

                                if (qtdNumSorteRegular <= 0)
                                {
                                    return BadRequest(new { success = false, message = $"O campo 'QtdNumSorteRegular' na linha {row} deve ser maior que zero." });
                                }

                                arquivos.Add(new Arquivo
                                {
                                    idCliente = idCliente,
                                    QtdNumSorteRegular = qtdNumSorteRegular,
                                    ArquivoNome = fileName
                                });

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
                                return BadRequest(new { success = false, message = "O arquivo Excel está vazio ou não contém dados." });
                            }

                            var colunasEsperadas = new[] { "idCliente", "QtdNumSorteRegular"};
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
                                return BadRequest(new { success = false, message = "Os títulos das colunas não correspondem ao esperado." });
                            }

                            for (int row = 2; row <= rowCount; row++)
                            {
                                var idClienteStr = worksheet.Cells[row, 1].Text;
                                var qtdNumSorteRegularStr = worksheet.Cells[row, 2].Text;

                                if (!int.TryParse(idClienteStr, out int idCliente))
                                {
                                    return BadRequest(new { success = false, message = $"O campo 'idCliente' na linha {row} não é um número válido." });
                                }

                                if (!int.TryParse(qtdNumSorteRegularStr, out int qtdNumSorteRegular))
                                {
                                    return BadRequest(new { success = false, message = $"O campo 'QtdNumSorteRegular' na linha {row} não é um número válido." });
                                }

                                if (qtdNumSorteRegular <= 0)
                                {
                                    return BadRequest(new { success = false, message = $"O campo 'QtdNumSorteRegular' na linha {row} deve ser maior que zero." });
                                }
                                
                                arquivos.Add(new Arquivo
                                {
                                    idCliente = idCliente,
                                    QtdNumSorteRegular = qtdNumSorteRegular,
                                    ArquivoNome = fileName
                                }); 
                            }
                        }
                    }
                }

                await _context.Arquivos.AddRangeAsync(arquivos);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Arquivo processado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Erro interno no servidor: {ex.Message}" });
            }
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

                return Ok(new { success = true, message = "Arquivo SQL processado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Erro interno no servidor: {ex.Message}" });
            }
        }
    }
}
