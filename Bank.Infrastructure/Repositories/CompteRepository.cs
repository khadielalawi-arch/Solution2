// Bank.Infrastructure/Repositories/CompteRepository.cs
using Bank.Application.Interfaces;
using Bank.Domain.Entities;
using Bank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bank.Infrastructure.Repositories;

public class CompteRepository : ICompteRepository
{
    private readonly AppDbContext _context;

    public CompteRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Compte>> GetAll() =>
        await _context.Comptes
            .Include(c => c.Client)
            .OrderBy(c => c.DateCreation)
            .ToListAsync();

    public async Task<Compte?> GetById(int compteId) =>
        await _context.Comptes
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.CompteId == compteId);

    public async Task<Compte> Add(Compte c)
    {
        c.DateCreation = DateTime.UtcNow;
        _context.Comptes.Add(c);
        await _context.SaveChangesAsync();
        return c;
    }

    public async Task<int> Nombre_op(int numeroCompte) =>
        await _context.Operations.CountAsync(o => o.CompteId == numeroCompte);

    public async Task<Compte?> update(Compte c)
    {
        var existing = await _context.Comptes.FindAsync(c.CompteId);
        if (existing == null) return null;

        existing.Solde = c.Solde;
        existing.ClientId = c.ClientId;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<Compte?> Delete(int compteId)
    {
        var compte = await _context.Comptes.FindAsync(compteId);
        if (compte == null) return null;

        _context.Comptes.Remove(compte);
        await _context.SaveChangesAsync();
        return compte;
    }

    public async Task<IEnumerable<Compte>> Search(int clientId) =>
        await _context.Comptes
            .Where(c => c.ClientId == clientId)
            .ToListAsync();

    public async Task<Operation> Add(Operation o)
    {
        o.DateOperation = DateTime.UtcNow;
        _context.Operations.Add(o);
        await _context.SaveChangesAsync();
        return o;
    }

    public async Task<Compte?> Maj_solde(int numcompte, Operation o)
    {
        var compte = await _context.Comptes.FindAsync(numcompte);
        if (compte == null) return null;

        if (o.Type == TypeOperation.Retrait) compte.Solde -= o.Montant;
        else if (o.Type == TypeOperation.Versement) compte.Solde += o.Montant;

        await _context.SaveChangesAsync();
        return compte;
    }

    public async Task<IEnumerable<Operation>> Extrait_Compte(int numcompte, DateTime d) =>
        await _context.Operations
            .Where(o => o.CompteId == numcompte && o.DateOperation >= d)
            .OrderBy(o => o.DateOperation)
            .ToListAsync();

    public async Task<double> Solde_Total(int clientId) =>
        await _context.Comptes
            .Where(c => c.ClientId == clientId)
            .SumAsync(c => c.Solde);

    public async Task<bool> MAJVirement(int numcompte1, int numcompte2, double m)
    {
        if (m <= 0 || numcompte1 == numcompte2) return false;

        await using var tx = await _context.Database.BeginTransactionAsync();

        var source = await _context.Comptes.FindAsync(numcompte1);
        var dest = await _context.Comptes.FindAsync(numcompte2);
        if (source == null || dest == null || source.Solde < m) return false;

        source.Solde -= m;
        dest.Solde += m;

        _context.Operations.Add(new Operation
        {
            CompteId = numcompte1,
            Type = TypeOperation.Virement,
            Montant = m,
            DateOperation = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return true;
    }

    public async Task<double> Total_Retraits(int clientId) =>
        await _context.Operations
            .Where(o => o.Type == TypeOperation.Retrait && o.Compte!.ClientId == clientId)
            .SumAsync(o => o.Montant);

    public async Task<bool> ClientExists(int clientId) =>
        await _context.Clients.AnyAsync(c => c.ClientId == clientId);
}