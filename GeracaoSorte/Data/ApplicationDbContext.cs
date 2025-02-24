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
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ParticipacoesSorte> ParticipacoesSorte { get; set; }

    }
}
