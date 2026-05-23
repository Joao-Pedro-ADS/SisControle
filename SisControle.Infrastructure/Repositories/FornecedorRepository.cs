using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class FornecedorRepository : IFornecedorRepository
{
    private readonly AppDbContext _ctx;
    public FornecedorRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Fornecedor>> GetAllAsync() =>
        await _ctx.Fornecedores.OrderBy(f => f.Nome).ToListAsync();

    public async Task<Fornecedor?> GetByIdAsync(int id) =>
        await _ctx.Fornecedores.FindAsync(id);

    public async Task<bool> CnpjExisteAsync(string cnpj, int? ignorarId = null) =>
        await _ctx.Fornecedores.AnyAsync(f => f.Cnpj == cnpj && (ignorarId == null || f.Id != ignorarId));

    public async Task AddAsync(Fornecedor fornecedor)
    {
        _ctx.Fornecedores.Add(fornecedor);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Fornecedor fornecedor)
    {
        _ctx.Fornecedores.Update(fornecedor);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var f = await _ctx.Fornecedores.FindAsync(id);
        if (f != null)
        {
            _ctx.Fornecedores.Remove(f);
            await _ctx.SaveChangesAsync();
        }
    }
}
