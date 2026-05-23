using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _ctx;
    public ClienteRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Cliente>> GetAllAsync() =>
        await _ctx.Clientes.OrderBy(c => c.Nome).ToListAsync();

    public async Task<Cliente?> GetByIdAsync(int id) =>
        await _ctx.Clientes.FindAsync(id);

    public async Task<Cliente?> GetByCpfAsync(string cpf) =>
        await _ctx.Clientes.FirstOrDefaultAsync(c => c.Cpf == cpf);

    public async Task AddAsync(Cliente cliente)
    {
        _ctx.Clientes.Add(cliente);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cliente cliente)
    {
        _ctx.Clientes.Update(cliente);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var c = await _ctx.Clientes.FindAsync(id);
        if (c != null)
        {
            _ctx.Clientes.Remove(c);
            await _ctx.SaveChangesAsync();
        }
    }
}
