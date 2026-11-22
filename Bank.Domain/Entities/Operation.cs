// Bank.Domain/Entities/Operation.cs
using System.ComponentModel.DataAnnotations;

namespace Bank.Domain.Entities;

public class Operation
{
    public int OperationId { get; set; }

    [Required]
    public TypeOperation Type { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être > 0")]
    public double Montant { get; set; }

    public DateTime DateOperation { get; set; } = DateTime.UtcNow;

    [Required]
    public int CompteId { get; set; }
    public Compte? Compte { get; set; }
}