using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class FuncionarioRepository : IFuncionarioRepository
{
    private readonly AppDbContext _ctx;
    public FuncionarioRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Funcionario>> GetAllAsync() =>
        await _ctx.Funcionarios.Include(f => f.Filial).OrderBy(f => f.Nome).ToListAsync();

    public async Task<Funcionario?> GetByIdAsync(int id) =>
        await _ctx.Funcionarios.Include(f => f.Filial).FirstOrDefaultAsync(f => f.Id == id);

    public async Task<IEnumerable<Funcionario>> GetByFilialAsync(int filialId) =>
        await _ctx.Funcionarios.Where(f => f.FilialId == filialId).OrderBy(f => f.Nome).ToListAsync();

    public async Task<bool> CpfExisteAsync(string cpf, int? ignorarId = null) =>
        await _ctx.Funcionarios.AnyAsync(f => f.Cpf == cpf && (ignorarId == null || f.Id != ignorarId));

    public async Task AddAsync(Funcionario funcionario)
    {
        _ctx.Funcionarios.Add(funcionario);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Funcionario funcionario)
    {
        _ctx.Funcionarios.Update(funcionario);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var f = await _ctx.Funcionarios.FindAsync(id);
        if (f != null)
        {
            _ctx.Funcionarios.Remove(f);
            await _ctx.SaveChangesAsync();
        }
    }
}
