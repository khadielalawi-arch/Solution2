// Bank.Domain/Entities/Compte.cs
using System.ComponentModel.DataAnnotations;

namespace Bank.Domain.Entities;

public class Compte
{
    public int CompteId { get; set; }          // = numéro de compte

    [Range(0, double.MaxValue, ErrorMessage = "Le solde doit être positif")]
    public double Solde { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    [Required]
    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public List<Operation> Operations { get; set; } = new();
}