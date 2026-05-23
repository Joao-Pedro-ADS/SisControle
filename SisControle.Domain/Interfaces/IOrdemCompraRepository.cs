using SisControle.Domain.Entities;

namespace SisControle.Domain.Interfaces;

public interface IOrdemCompraRepository
{
    Task<IEnumerable<OrdemCompra>> GetByFilialAsync(int filialId);
    Task<OrdemCompra?> GetByIdAsync(int id);
    Task AddAsync(OrdemCompra ordemCompra);
    Task UpdateAsync(OrdemCompra ordemCompra);
}
