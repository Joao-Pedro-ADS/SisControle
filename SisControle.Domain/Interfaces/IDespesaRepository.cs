using SisControle.Domain.Entities;

namespace SisControle.Domain.Interfaces;

public interface IDespesaRepository
{
    Task<IEnumerable<DespesaOperacional>> GetByFilialAsync(int filialId);
    Task<IEnumerable<DespesaOperacional>> GetByFilialEPeriodoAsync(int filialId, DateTime inicio, DateTime fim);
    Task AddAsync(DespesaOperacional despesa);
    Task UpdateAsync(DespesaOperacional despesa);
    Task DeleteAsync(int id);
}
