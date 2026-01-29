using Microsoft.EntityFrameworkCore;
// Force rebuild
using RuralAddress.Core.Entities;
using RuralAddress.Core.Interfaces;
using RuralAddress.Infrastructure.Data;

namespace RuralAddress.Infrastructure.Services;

public class PessoaService : IPessoaService
{
    private readonly AppDbContext _context;

    public PessoaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Pessoa>> GetAllAsync()
    {
        return await _context.Pessoas
            .Include(p => p.Propriedade)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pessoa>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllAsync();

        term = term.ToLower();
        return await _context.Pessoas
            .Include(p => p.Propriedade)
            .Where(p => p.Nome.ToLower().Contains(term) || 
                        p.Cpf.Contains(term))
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetSuggestionsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return new List<string>();

        term = term.ToLower();
        return await _context.Pessoas
            .Where(p => p.Nome.ToLower().Contains(term))
            .Select(p => p.Nome)
            .Take(5)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pessoa>> GetByPropriedadeIdAsync(int propriedadeId)
    {
        return await _context.Pessoas
            .Where(p => p.PropriedadeId == propriedadeId)
            .ToListAsync();
    }

    public async Task<Pessoa?> GetByIdAsync(int id)
    {
        return await _context.Pessoas.FindAsync(id);
    }

    public async Task<Pessoa> AddAsync(Pessoa pessoa)
    {
        if (pessoa.Nascimento.HasValue)
        {
            pessoa.Nascimento = DateTime.SpecifyKind(pessoa.Nascimento.Value, DateTimeKind.Utc);
        }
        _context.Pessoas.Add(pessoa);
        await _context.SaveChangesAsync();
        return pessoa;
    }

    public async Task UpdateAsync(Pessoa pessoa)
    {
        if (pessoa.Nascimento.HasValue)
        {
            pessoa.Nascimento = DateTime.SpecifyKind(pessoa.Nascimento.Value, DateTimeKind.Utc);
        }
        _context.Entry(pessoa).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var pessoa = await _context.Pessoas.FindAsync(id);
        if (pessoa != null)
        {
            _context.Pessoas.Remove(pessoa);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string?> ValidateCpfUniquenessAsync(string cpf, string nome, string rg, int? currentId = null)
    {
        if (string.IsNullOrEmpty(cpf)) return null;

        // Find any other person with the same CPF
        var existingPerson = await _context.Pessoas
            .Where(p => p.Cpf == cpf && (currentId == null || p.Id != currentId))
            .FirstOrDefaultAsync();

        if (existingPerson != null)
        {
            // If CPF exists, Name and RG MUST match
            if (!string.Equals(existingPerson.Nome, nome, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(existingPerson.Rg, rg, StringComparison.OrdinalIgnoreCase))
            {
                return "Este CPF já está cadastrado para outra pessoa (Nome ou RG diferentes).";
            }
        }

        return null;
    }
}
