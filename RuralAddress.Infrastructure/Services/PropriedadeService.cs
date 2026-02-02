using Microsoft.EntityFrameworkCore;
using RuralAddress.Core.Entities;
using RuralAddress.Core.Interfaces;
using RuralAddress.Infrastructure.Data;

namespace RuralAddress.Infrastructure.Services;

public class PropriedadeService : IPropriedadeService
{
    private readonly AppDbContext _context;

    public PropriedadeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Propriedade>> GetAllAsync()
    {
        return await _context.Propriedades
            .Include(p => p.Pessoas)
            .Include(p => p.Veiculos)
            .Include(p => p.Cultivos)
                .ThenInclude(pc => pc.Cultivo)
            .OrderBy(p => p.CepRural)
            .ToListAsync();
    }

    public async Task<List<Propriedade>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllAsync();

        term = term.ToLower();
        return await _context.Propriedades
            .Include(p => p.Pessoas)
            .Include(p => p.Veiculos)
            .Include(p => p.Cultivos)
                .ThenInclude(pc => pc.Cultivo)
            .Where(p => p.NomePropriedade.ToLower().Contains(term) ||
                        p.CepRural.ToLower().Contains(term) ||
                        p.Bairro.ToLower().Contains(term) ||
                        p.Pessoas.Any(pe => pe.Nome.ToLower().Contains(term)) ||
                        p.Veiculos.Any(v => v.Marca.ToLower().Contains(term) || 
                                            v.Modelo.ToLower().Contains(term) || 
                                            v.Placa.ToLower().Contains(term)))
            .OrderBy(p => p.CepRural)
            .ToListAsync();
    }

    public async Task<List<string>> GetSuggestionsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return new List<string>();

        term = term.ToLower();

        var propSuggestions = await _context.Propriedades
            .Where(p => p.NomePropriedade.ToLower().Contains(term))
            .Select(p => p.NomePropriedade)
            .Take(5)
            .ToListAsync();

        var pessoaSuggestions = await _context.Pessoas
            .Where(p => p.Nome.ToLower().Contains(term))
            .Select(p => p.Nome)
            .Take(5)
            .ToListAsync();

        var veiculoSuggestions = await _context.Veiculos
            .Where(v => v.Marca.ToLower().Contains(term) ||
                        v.Modelo.ToLower().Contains(term) ||
                        v.Placa.ToLower().Contains(term))
            .Select(v => v.Marca + " " + v.Modelo)
            .Take(5)
            .ToListAsync();

        // Also add CEPs if they match
        var cepSuggestions = await _context.Propriedades
             .Where(p => p.CepRural.ToLower().Contains(term))
             .Select(p => p.CepRural)
             .Take(3)
             .ToListAsync();

        var bairroSuggestions = await _context.Propriedades
             .Where(p => p.Bairro.ToLower().Contains(term))
             .Select(p => p.Bairro)
             .Distinct()
             .Take(3)
             .ToListAsync();

        var allSuggestions = propSuggestions
            .Concat(pessoaSuggestions)
            .Concat(veiculoSuggestions)
            .Concat(cepSuggestions)
            .Concat(bairroSuggestions)
            .Distinct()
            .Take(10)
            .ToList();

        return allSuggestions;
    }

    public async Task<List<string>> GetScopedSuggestionsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return new List<string>();

        term = term.ToLower();

        var propSuggestions = await _context.Propriedades
            .Where(p => p.NomePropriedade.ToLower().Contains(term))
            .Select(p => p.NomePropriedade)
            .Take(5)
            .ToListAsync();

        var cepSuggestions = await _context.Propriedades
             .Where(p => p.CepRural.ToLower().Contains(term))
             .Select(p => p.CepRural)
             .Take(3)
             .ToListAsync();

        var bairroSuggestions = await _context.Propriedades
             .Where(p => p.Bairro.ToLower().Contains(term))
             .Select(p => p.Bairro)
             .Distinct()
             .Take(3)
             .ToListAsync();

        return propSuggestions
            .Concat(cepSuggestions)
            .Concat(bairroSuggestions)
            .Distinct()
            .Take(10)
            .ToList();
    }

    public async Task<Propriedade?> GetByIdAsync(int id)
    {
        return await _context.Propriedades
            .Include(p => p.Pessoas)
            .Include(p => p.Veiculos)
            .Include(p => p.Cultivos)
                .ThenInclude(pc => pc.Cultivo)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Propriedade> AddAsync(Propriedade propriedade)
    {
        _context.Propriedades.Add(propriedade);
        await _context.SaveChangesAsync();
        return propriedade;
    }

    public async Task UpdateAsync(Propriedade propriedade)
    {
        _context.Entry(propriedade).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var propriedade = await _context.Propriedades.FindAsync(id);
        if (propriedade != null)
        {
            _context.Propriedades.Remove(propriedade);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<List<Propriedade>> GetBySetorAsync(int setor)
    {
        return await _context.Propriedades
            .Include(p => p.Pessoas)
            .Include(p => p.Veiculos)
            .Where(p => p.Setor == setor)
            .OrderBy(p => p.CepRural)
            .ToListAsync();
    }
}
