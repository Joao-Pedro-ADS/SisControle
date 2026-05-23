using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class ClienteService
{
    private readonly IClienteRepository _repo;
    public ClienteService(IClienteRepository repo) => _repo = repo;

    public Task<IEnumerable<Cliente>> ListarAsync() => _repo.GetAllAsync();
    public Task<Cliente?> ObterAsync(int id) => _repo.GetByIdAsync(id);
    public Task<Cliente?> ObterPorCpfAsync(string cpf) => _repo.GetByCpfAsync(cpf);

    public async Task CadastrarAsync(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nome))
            throw new InvalidOperationException("Nome do cliente é obrigatório.");
        await _repo.AddAsync(cliente);
    }

    public Task AtualizarAsync(Cliente cliente) => _repo.UpdateAsync(cliente);
    public Task ExcluirAsync(int id) => _repo.DeleteAsync(id);
}
