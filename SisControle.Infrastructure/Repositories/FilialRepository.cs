using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class FilialRepository : IFilialRepository
{
    private readonly AppDbContext _ctx;
    public FilialRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Filial>> GetAllAsync() =>
        await _ctx.Filiais.OrderBy(f => f.Nome).ToListAsync();

    public async Task<Filial?> GetByIdAsync(int id) =>
        await _ctx.Filiais.FindAsync(id);

    public async Task<IEnumerable<Filial>> GetByEstadoAsync(string estado) =>
        await _ctx.Filiais.Where(f => f.Estado == estado).ToListAsync();

    public async Task AddAsync(Filial filial)
    {
        _ctx.Filiais.Add(filial);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Filial filial)
    {
        _ctx.Filiais.Update(filial);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var filial = await _ctx.Filiais.FindAsync(id);
        if (filial != null)
        {
            _ctx.Filiais.Remove(filial);
            await _ctx.SaveChangesAsync();
        }
    }
}
