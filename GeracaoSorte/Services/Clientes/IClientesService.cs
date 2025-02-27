using GeracaoSorte.Data;

namespace GeracaoSorte.Services.Clientes
{
    public interface IClientesService
    {
        Task<List<ParticipacoesSorte>> GerarNumerosSorte(int quantidade, int idCliente);
        Task<List<ClienteComNumeros>> GetClientesPorArquivoNome(string arquivoNome);

        Task<List<string>> GetTodosArquivoNome();
        Task SalvarClientesComNumeros(List<ClienteComNumeros> clientes);
    }
}
