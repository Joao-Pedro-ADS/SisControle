using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class FuncionarioService
{
    private readonly IFuncionarioRepository _repo;
    public FuncionarioService(IFuncionarioRepository repo) => _repo = repo;

    public Task<IEnumerable<Funcionario>> ListarAsync() => _repo.GetAllAsync();
    public Task<Funcionario?> ObterAsync(int id) => _repo.GetByIdAsync(id);
    public Task<IEnumerable<Funcionario>> ListarPorFilialAsync(int filialId) => _repo.GetByFilialAsync(filialId);

    public async Task CadastrarAsync(Funcionario funcionario)
    {
        if (string.IsNullOrWhiteSpace(funcionario.Nome))
            throw new InvalidOperationException("Nome do funcionário é obrigatório.");

        // Regra 14 — CPF de funcionário é único na rede inteira
        if (await _repo.CpfExisteAsync(funcionario.Cpf))
            throw new InvalidOperationException("Já existe um funcionário com este CPF em qualquer filial da rede.");

        await _repo.AddAsync(funcionario);
    }

    public async Task AtualizarAsync(Funcionario funcionario)
    {
        if (string.IsNullOrWhiteSpace(funcionario.Nome))
            throw new InvalidOperationException("Nome do funcionário é obrigatório.");

        if (await _repo.CpfExisteAsync(funcionario.Cpf, funcionario.Id))
            throw new InvalidOperationException("Já existe outro funcionário com este CPF na rede.");

        await _repo.UpdateAsync(funcionario);
    }

    public Task ExcluirAsync(int id) => _repo.DeleteAsync(id);
}
