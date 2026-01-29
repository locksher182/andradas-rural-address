using Microsoft.EntityFrameworkCore;
using RuralAddress.Core.Entities;
using RuralAddress.Core.Interfaces;
using RuralAddress.Infrastructure.Data;

namespace RuralAddress.Infrastructure.Services;

public class SystemParameterService : ISystemParameterService
{
    private readonly AppDbContext _context;

    public SystemParameterService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SystemParameter>> GetByGroupAsync(string group)
    {
        return await _context.SystemParameters
            .Where(p => p.Group == group && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemParameter>> GetByGroupAllAsync(string group)
    {
        return await _context.SystemParameters
            .Where(p => p.Group == group)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<SystemParameter?> GetByIdAsync(int id)
    {
        return await _context.SystemParameters.FindAsync(id);
    }

    public async Task<SystemParameter> AddAsync(SystemParameter parameter)
    {
        _context.SystemParameters.Add(parameter);
        await _context.SaveChangesAsync();
        return parameter;
    }

    public async Task UpdateAsync(SystemParameter parameter)
    {
        _context.Entry(parameter).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var parameter = await _context.SystemParameters.FindAsync(id);
        if (parameter != null)
        {
            _context.SystemParameters.Remove(parameter);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<SystemParameter>> GetAllAsync()
    {
        return await _context.SystemParameters.OrderBy(p => p.Group).ThenBy(p => p.Name).ToListAsync();
    }
}
