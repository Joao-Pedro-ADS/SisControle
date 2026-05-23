using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class CategoriaService
{
    private readonly ICategoriaRepository _repo;
    public CategoriaService(ICategoriaRepository repo) => _repo = repo;

    public Task<IEnumerable<Categoria>> ListarAsync() => _repo.GetAllAsync();
    public Task<Categoria?> ObterAsync(int id) => _repo.GetByIdAsync(id);

    public async Task CadastrarAsync(Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nome))
            throw new InvalidOperationException("Nome da categoria é obrigatório.");
        await _repo.AddAsync(categoria);
    }

    public Task AtualizarAsync(Categoria categoria) => _repo.UpdateAsync(categoria);
    public Task ExcluirAsync(int id) => _repo.DeleteAsync(id);
}
