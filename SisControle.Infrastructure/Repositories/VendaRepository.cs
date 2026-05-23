using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class VendaRepository : IVendaRepository
{
    private readonly AppDbContext _ctx;
    public VendaRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Venda>> GetAllAsync() =>
        await _ctx.Vendas
            .Include(v => v.Filial)
            .Include(v => v.Cliente)
            .Include(v => v.Funcionario)
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .OrderByDescending(v => v.DataHora)
            .ToListAsync();

    public async Task<Venda?> GetByIdAsync(int id) =>
        await _ctx.Vendas
            .Include(v => v.Filial)
            .Include(v => v.Cliente)
            .Include(v => v.Funcionario)
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<Venda>> GetByFilialAsync(int filialId) =>
        await _ctx.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .Where(v => v.FilialId == filialId)
            .OrderByDescending(v => v.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<Venda>> GetByPeriodoAsync(DateTime inicio, DateTime fim) =>
        await _ctx.Vendas
            .Include(v => v.Filial)
            .Include(v => v.Itens)
            .Where(v => v.DataHora >= inicio && v.DataHora <= fim)
            .OrderByDescending(v => v.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<Venda>> GetByFilialEPeriodoAsync(int filialId, DateTime inicio, DateTime fim) =>
        await _ctx.Vendas
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .Where(v => v.FilialId == filialId && v.DataHora >= inicio && v.DataHora <= fim)
            .OrderByDescending(v => v.DataHora)
            .ToListAsync();

    public async Task AddAsync(Venda venda)
    {
        _ctx.Vendas.Add(venda);
        await _ctx.SaveChangesAsync();
    }
}
