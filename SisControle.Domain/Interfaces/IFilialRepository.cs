using SisControle.Domain.Entities;

namespace SisControle.Domain.Interfaces;

public interface IFilialRepository
{
    Task<IEnumerable<Filial>> GetAllAsync();
    Task<Filial?> GetByIdAsync(int id);
    Task<IEnumerable<Filial>> GetByEstadoAsync(string estado);
    Task AddAsync(Filial filial);
    Task UpdateAsync(Filial filial);
    Task DeleteAsync(int id);
}
