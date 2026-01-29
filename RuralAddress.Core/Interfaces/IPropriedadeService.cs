using RuralAddress.Core.Entities;

namespace RuralAddress.Core.Interfaces;

public interface IPropriedadeService
{
    Task<List<Propriedade>> GetAllAsync();
    Task<List<Propriedade>> SearchAsync(string term);
    Task<List<string>> GetSuggestionsAsync(string term);
    Task<List<string>> GetScopedSuggestionsAsync(string term);
    Task<Propriedade?> GetByIdAsync(int id);
    Task<Propriedade> AddAsync(Propriedade propriedade);
    Task<List<Propriedade>> GetBySetorAsync(int setor);
    Task UpdateAsync(Propriedade propriedade);
    Task DeleteAsync(int id);
}
