// Bank.Domain/Entities/Client.cs
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Bank.Domain.Entities;

public class Client
{
    public int ClientId { get; set; }

    [Required, StringLength(50)]
    public string Nom { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Prenom { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public List<Compte> Comptes { get; set; } = new();
}