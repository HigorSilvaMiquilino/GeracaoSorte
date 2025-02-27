using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GeracaoSorte.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Arquivo> Arquivos { get; set; }

        public DbSet<ClienteComNumeros> ClienteComNumeros { get; set; }
        public DbSet<ParticipacoesSorte> ParticipacoesSorte { get; set; }

        public DbSet<LogErro> LogsErro { get; set; }

        public DbSet<LogSucesso> LogsSucesso { get; set; }

        public DbSet<ProgressoGeracao> ProgressoGeracoes { get; set; }

    }
}
