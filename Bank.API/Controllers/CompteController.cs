// Bank.API/Controllers/CompteController.cs
using Bank.Application.Interfaces;
using Bank.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Bank.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CompteController : ControllerBase
{
    private readonly ICompteRepository _repo;

    public CompteController(ICompteRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Compte>>> GetComptes() =>
        Ok(await _repo.GetAll());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Compte>> GetCompte(int id)
    {
        var compte = await _repo.GetById(id);
        return compte == null ? NotFound() : Ok(compte);
    }

    [HttpPost]
    public async Task<ActionResult<Compte>> CreateCompte(Compte c)
    {
        if (!await _repo.ClientExists(c.ClientId))
            return BadRequest("Le client spécifié n'existe pas.");

        var created = await _repo.Add(c);
        return CreatedAtAction(nameof(GetCompte), new { id = created.CompteId }, created);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Compte>> DeleteCompte(int id)
    {
        if (await _repo.GetById(id) == null) return NotFound();

        if (await _repo.Nombre_op(id) > 0)
            return BadRequest("Suppression interdite : le compte a des opérations.");

        return Ok(await _repo.Delete(id));
    }

    [HttpGet("search/{clientId:int}")]
    public async Task<ActionResult<IEnumerable<Compte>>> Search(int clientId) =>
        Ok(await _repo.Search(clientId));

    [HttpPost("operation")]
    public async Task<ActionResult<Operation>> CreateOperation(Operation p)
    {
        var compte = await _repo.GetById(p.CompteId);
        if (compte == null) return NotFound("Compte introuvable.");

        if (p.Type == TypeOperation.Virement)
            return BadRequest("Utilisez /api/Compte/virement pour un virement.");

        if (p.Type == TypeOperation.Retrait && compte.Solde < p.Montant)
            return BadRequest("Solde insuffisant.");

        var op = await _repo.Add(p);
        await _repo.Maj_solde(p.CompteId, p);
        return Ok(op);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Compte>> UpdateCompte(int id, Compte c)
    {
        if (id != c.CompteId) return BadRequest("L'id ne correspond pas.");

        var updated = await _repo.update(c);
        return updated == null ? NotFound() : Ok(updated);
    }

    // Nombre d'opérations d'un compte
    [HttpGet("{id:int}/nombre-operations")]
    public async Task<ActionResult<int>> NombreOperations(int id)
    {
        if (await _repo.GetById(id) == null) return NotFound();
        return Ok(await _repo.Nombre_op(id));
    }

    [HttpGet("{num:int}/extrait")]
    public async Task<ActionResult<IEnumerable<Operation>>> Extrait(int num, DateTime date)
    {
        if (await _repo.GetById(num) == null) return NotFound();
        return Ok(await _repo.Extrait_Compte(num, date));
    }

    [HttpGet("solde-total/{clientId:int}")]
    public async Task<ActionResult<double>> SoldeTotal(int clientId)
    {
        if (!await _repo.ClientExists(clientId)) return NotFound();
        return Ok(await _repo.Solde_Total(clientId));
    }

    [HttpPost("virement")]
    public async Task<ActionResult> MAJVirement(int num1, int num2, double montant)
    {
        var ok = await _repo.MAJVirement(num1, num2, montant);
        return ok ? Ok("Virement effectué.")
                  : BadRequest("Virement impossible (comptes invalides ou solde insuffisant).");
    }

    [HttpGet("total-retraits/{clientId:int}")]
    public async Task<ActionResult<double>> TotalRetraits(int clientId)
    {
        if (!await _repo.ClientExists(clientId)) return NotFound();
        return Ok(await _repo.Total_Retraits(clientId));
    }
}