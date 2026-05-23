using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class DespesaRepository : IDespesaRepository
{
    private readonly AppDbContext _ctx;
    public DespesaRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<DespesaOperacional>> GetByFilialAsync(int filialId) =>
        await _ctx.DespesasOperacionais
            .Include(d => d.Filial)
            .Where(d => d.FilialId == filialId)
            .OrderByDescending(d => d.DataLancamento)
            .ToListAsync();

    public async Task<IEnumerable<DespesaOperacional>> GetByFilialEPeriodoAsync(int filialId, DateTime inicio, DateTime fim) =>
        await _ctx.DespesasOperacionais
            .Where(d => d.FilialId == filialId && d.DataLancamento >= inicio && d.DataLancamento <= fim)
            .ToListAsync();

    public async Task AddAsync(DespesaOperacional despesa)
    {
        _ctx.DespesasOperacionais.Add(despesa);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(DespesaOperacional despesa)
    {
        _ctx.DespesasOperacionais.Update(despesa);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var d = await _ctx.DespesasOperacionais.FindAsync(id);
        if (d != null)
        {
            _ctx.DespesasOperacionais.Remove(d);
            await _ctx.SaveChangesAsync();
        }
    }
}
