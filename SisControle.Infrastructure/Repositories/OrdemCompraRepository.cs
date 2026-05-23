using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class OrdemCompraRepository : IOrdemCompraRepository
{
    private readonly AppDbContext _ctx;
    public OrdemCompraRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<OrdemCompra>> GetByFilialAsync(int filialId) =>
        await _ctx.OrdensCompra
            .Include(o => o.Fornecedor)
            .Include(o => o.Itens).ThenInclude(i => i.Produto)
            .Where(o => o.FilialId == filialId)
            .OrderByDescending(o => o.DataPedido)
            .ToListAsync();

    public async Task<OrdemCompra?> GetByIdAsync(int id) =>
        await _ctx.OrdensCompra
            .Include(o => o.Fornecedor)
            .Include(o => o.Filial)
            .Include(o => o.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task AddAsync(OrdemCompra ordemCompra)
    {
        _ctx.OrdensCompra.Add(ordemCompra);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(OrdemCompra ordemCompra)
    {
        _ctx.OrdensCompra.Update(ordemCompra);
        await _ctx.SaveChangesAsync();
    }
}
