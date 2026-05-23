using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _ctx;
    public CategoriaRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Categoria>> GetAllAsync() =>
        await _ctx.Categorias.OrderBy(c => c.Nome).ToListAsync();

    public async Task<Categoria?> GetByIdAsync(int id) =>
        await _ctx.Categorias.FindAsync(id);

    public async Task AddAsync(Categoria categoria)
    {
        _ctx.Categorias.Add(categoria);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        _ctx.Categorias.Update(categoria);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var cat = await _ctx.Categorias.FindAsync(id);
        if (cat != null)
        {
            _ctx.Categorias.Remove(cat);
            await _ctx.SaveChangesAsync();
        }
    }
}
