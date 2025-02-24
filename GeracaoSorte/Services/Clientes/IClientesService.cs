using GeracaoSorte.Data;

namespace GeracaoSorte.Services.Clientes
{
    public interface IClientesService
    {
        Task<List<ClienteComNumeros>> GetClientesPorArquivoNome(string arquivoNome);

        Task<List<string>> GetTodosArquivoNome();
    }
}
