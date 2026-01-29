using RuralAddress.Core.Entities;
// Force rebuild

namespace RuralAddress.Core.Interfaces;

public interface IPessoaService
{
    Task<IEnumerable<Pessoa>> GetAllAsync();
    Task<IEnumerable<Pessoa>> SearchAsync(string term);
    Task<IEnumerable<string>> GetSuggestionsAsync(string term);
    Task<IEnumerable<Pessoa>> GetByPropriedadeIdAsync(int propriedadeId);
    Task<Pessoa?> GetByIdAsync(int id);
    Task<Pessoa> AddAsync(Pessoa pessoa);
    Task UpdateAsync(Pessoa pessoa);
    Task DeleteAsync(int id);
    Task<string?> ValidateCpfUniquenessAsync(string cpf, string nome, string rg, int? currentId = null);
}
