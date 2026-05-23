using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class ProdutoService
{
    private readonly IProdutoRepository _repo;
    private readonly ICategoriaRepository _categoriaRepo;

    public ProdutoService(IProdutoRepository repo, ICategoriaRepository categoriaRepo)
    {
        _repo = repo;
        _categoriaRepo = categoriaRepo;
    }

    public Task<IEnumerable<Produto>> ListarAsync() => _repo.GetAllAsync();
    public Task<Produto?> ObterAsync(int id) => _repo.GetByIdAsync(id);
    public Task<Produto?> ObterPorCodigoBarrasAsync(string codigo) => _repo.GetByCodigoBarrasAsync(codigo);

    public async Task CadastrarAsync(Produto produto)
    {
        // Regra 13 — Produto sem categoria não pode ser cadastrado
        var categoria = await _categoriaRepo.GetByIdAsync(produto.CategoriaId);
        if (categoria == null)
            throw new InvalidOperationException("Categoria informada não existe. Cadastre a categoria primeiro.");

        if (string.IsNullOrWhiteSpace(produto.Nome))
            throw new InvalidOperationException("Nome do produto é obrigatório.");
        if (produto.PrecoVenda <= 0)
            throw new InvalidOperationException("Preço de venda deve ser maior que zero.");
        if (produto.PrecoCusto < 0)
            throw new InvalidOperationException("Preço de custo não pode ser negativo.");

        await _repo.AddAsync(produto);
    }

    public async Task AtualizarAsync(Produto produto)
    {
        var categoria = await _categoriaRepo.GetByIdAsync(produto.CategoriaId);
        if (categoria == null)
            throw new InvalidOperationException("Categoria informada não existe.");
        await _repo.UpdateAsync(produto);
    }

    public Task ExcluirAsync(int id) => _repo.DeleteAsync(id);
}
