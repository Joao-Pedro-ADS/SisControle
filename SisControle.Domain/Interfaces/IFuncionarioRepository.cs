using SisControle.Domain.Entities;

namespace SisControle.Domain.Interfaces;

public interface IFuncionarioRepository
{
    Task<IEnumerable<Funcionario>> GetAllAsync();
    Task<Funcionario?> GetByIdAsync(int id);
    Task<IEnumerable<Funcionario>> GetByFilialAsync(int filialId);
    Task<bool> CpfExisteAsync(string cpf, int? ignorarId = null);
    Task AddAsync(Funcionario funcionario);
    Task UpdateAsync(Funcionario funcionario);
    Task DeleteAsync(int id);
}
