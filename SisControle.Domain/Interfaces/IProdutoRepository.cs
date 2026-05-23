using SisControle.Domain.Entities;

namespace SisControle.Domain.Interfaces;

public interface IProdutoRepository
{
    Task<IEnumerable<Produto>> GetAllAsync();
    Task<Produto?> GetByIdAsync(int id);
    Task<Produto?> GetByCodigoBarrasAsync(string codigoBarras);
    Task<IEnumerable<Produto>> GetByCategoriaAsync(int categoriaId);
    Task AddAsync(Produto produto);
    Task UpdateAsync(Produto produto);
    Task DeleteAsync(int id);
}
