using Microsoft.EntityFrameworkCore;
using RuralAddress.Core.Entities;
using RuralAddress.Core.Interfaces;
using RuralAddress.Infrastructure.Data;

namespace RuralAddress.Infrastructure.Services;

public class VeiculoService : IVeiculoService
{
    private readonly AppDbContext _context;

    public VeiculoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Veiculo>> GetAllAsync()
    {
        return await _context.Veiculos
            .Include(v => v.Propriedade)
            .Include(v => v.Tipo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Veiculo>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllAsync();

        term = term.ToLower();
        return await _context.Veiculos
            .Include(v => v.Propriedade)
            .Include(v => v.Tipo)
            .Where(v => v.Placa.ToLower().Contains(term) || 
                        v.Modelo.ToLower().Contains(term) || 
                        v.Marca.ToLower().Contains(term))
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetSuggestionsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return new List<string>();

        term = term.ToLower();
        return await _context.Veiculos
            .Where(v => v.Placa.ToLower().Contains(term) || 
                        v.Modelo.ToLower().Contains(term) || 
                        v.Marca.ToLower().Contains(term))
            .Select(v => v.Placa + " - " + v.Modelo)
            .Take(5)
            .ToListAsync();
    }

    public async Task<IEnumerable<Veiculo>> GetByPropriedadeIdAsync(int propriedadeId)
    {
        return await _context.Veiculos
            .Include(v => v.Tipo)
            .Where(v => v.PropriedadeId == propriedadeId)
            .ToListAsync();
    }

    public async Task<Veiculo?> GetByIdAsync(int id)
    {
        return await _context.Veiculos
            .Include(v => v.Tipo)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Veiculo> AddAsync(Veiculo veiculo)
    {
        _context.Veiculos.Add(veiculo);
        await _context.SaveChangesAsync();
        return veiculo;
    }

    public async Task UpdateAsync(Veiculo veiculo)
    {
        _context.Entry(veiculo).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var veiculo = await _context.Veiculos.FindAsync(id);
        if (veiculo != null)
        {
            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
        }
    }
}
