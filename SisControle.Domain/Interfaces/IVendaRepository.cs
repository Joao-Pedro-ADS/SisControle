using SisControle.Domain.Entities;

namespace SisControle.Domain.Interfaces;

public interface IVendaRepository
{
    Task<IEnumerable<Venda>> GetAllAsync();
    Task<Venda?> GetByIdAsync(int id);
    Task<IEnumerable<Venda>> GetByFilialAsync(int filialId);
    Task<IEnumerable<Venda>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
    Task<IEnumerable<Venda>> GetByFilialEPeriodoAsync(int filialId, DateTime inicio, DateTime fim);
    Task AddAsync(Venda venda);
}
