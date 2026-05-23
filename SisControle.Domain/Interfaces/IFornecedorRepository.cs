using SisControle.Domain.Entities;

namespace SisControle.Domain.Interfaces;

public interface IFornecedorRepository
{
    Task<IEnumerable<Fornecedor>> GetAllAsync();
    Task<Fornecedor?> GetByIdAsync(int id);
    Task<bool> CnpjExisteAsync(string cnpj, int? ignorarId = null);
    Task AddAsync(Fornecedor fornecedor);
    Task UpdateAsync(Fornecedor fornecedor);
    Task DeleteAsync(int id);
}
