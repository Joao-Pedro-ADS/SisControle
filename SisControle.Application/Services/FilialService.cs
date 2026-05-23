using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class FilialService
{
    private readonly IFilialRepository _repo;
    public FilialService(IFilialRepository repo) => _repo = repo;

    public Task<IEnumerable<Filial>> ListarAsync() => _repo.GetAllAsync();
    public Task<Filial?> ObterAsync(int id) => _repo.GetByIdAsync(id);
    public Task<IEnumerable<Filial>> ListarPorEstadoAsync(string estado) => _repo.GetByEstadoAsync(estado);

    public async Task CadastrarAsync(Filial filial)
    {
        if (string.IsNullOrWhiteSpace(filial.Nome))
            throw new InvalidOperationException("Nome da filial é obrigatório.");
        if (string.IsNullOrWhiteSpace(filial.Cnpj))
            throw new InvalidOperationException("CNPJ da filial é obrigatório.");
        if (string.IsNullOrWhiteSpace(filial.Estado))
            throw new InvalidOperationException("Estado da filial é obrigatório.");
        await _repo.AddAsync(filial);
    }

    public async Task AtualizarAsync(Filial filial)
    {
        if (string.IsNullOrWhiteSpace(filial.Nome))
            throw new InvalidOperationException("Nome da filial é obrigatório.");
        await _repo.UpdateAsync(filial);
    }

    public Task ExcluirAsync(int id) => _repo.DeleteAsync(id);
}
