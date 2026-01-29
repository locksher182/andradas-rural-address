using RuralAddress.Core.Entities;

namespace RuralAddress.Core.Interfaces;

public interface ISystemParameterService
{
    Task<IEnumerable<SystemParameter>> GetByGroupAsync(string group);
    Task<IEnumerable<SystemParameter>> GetByGroupAllAsync(string group);
    Task<SystemParameter?> GetByIdAsync(int id);
    Task<SystemParameter> AddAsync(SystemParameter parameter);
    Task UpdateAsync(SystemParameter parameter);
    Task DeleteAsync(int id);
    Task<IEnumerable<SystemParameter>> GetAllAsync();
}
