using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _ctx;
    public ProdutoRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Produto>> GetAllAsync() =>
        await _ctx.Produtos.Include(p => p.Categoria).OrderBy(p => p.Nome).ToListAsync();

    public async Task<Produto?> GetByIdAsync(int id) =>
        await _ctx.Produtos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Produto?> GetByCodigoBarrasAsync(string codigoBarras) =>
        await _ctx.Produtos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);

    public async Task<IEnumerable<Produto>> GetByCategoriaAsync(int categoriaId) =>
        await _ctx.Produtos.Include(p => p.Categoria).Where(p => p.CategoriaId == categoriaId).ToListAsync();

    public async Task AddAsync(Produto produto)
    {
        _ctx.Produtos.Add(produto);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Produto produto)
    {
        _ctx.Produtos.Update(produto);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var produto = await _ctx.Produtos.FindAsync(id);
        if (produto != null)
        {
            _ctx.Produtos.Remove(produto);
            await _ctx.SaveChangesAsync();
        }
    }
}
