// Bank.Infrastructure/Data/AppDbContext.cs
using Bank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; } = default!;
    public DbSet<Compte> Comptes { get; set; } = default!;
    public DbSet<Operation> Operations { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Compte>()
            .HasOne(c => c.Client)
            .WithMany(cl => cl.Comptes)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Operation>()
            .HasOne(o => o.Compte)
            .WithMany(c => c.Operations)
            .HasForeignKey(o => o.CompteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Operation>()
            .Property(o => o.Type)
            .HasConversion<string>();
    }
}