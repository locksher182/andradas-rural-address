using RuralAddress.Core.Entities;

namespace RuralAddress.Core.Interfaces;

public interface IVeiculoService
{
    Task<IEnumerable<Veiculo>> GetAllAsync();
    Task<IEnumerable<Veiculo>> SearchAsync(string term);
    Task<IEnumerable<string>> GetSuggestionsAsync(string term);
    Task<IEnumerable<Veiculo>> GetByPropriedadeIdAsync(int propriedadeId);
    Task<Veiculo?> GetByIdAsync(int id);
    Task<Veiculo> AddAsync(Veiculo veiculo);
    Task UpdateAsync(Veiculo veiculo);
    Task DeleteAsync(int id);
}
