using Microsoft.EntityFrameworkCore;

namespace DonationWorker.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Tabela propria do worker para registrar doacoes processadas
    public DbSet<Doacao> Doacoes => Set<Doacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Doacao>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Valor).HasPrecision(18, 2);
        });
    }
}
