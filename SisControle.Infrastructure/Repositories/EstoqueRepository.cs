using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;

namespace SisControle.Infrastructure.Repositories;

public class EstoqueRepository : IEstoqueRepository
{
    private readonly AppDbContext _ctx;
    public EstoqueRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<EstoqueFilial?> GetEstoqueAsync(int filialId, int produtoId) =>
        await _ctx.EstoqueFiliais
            .Include(e => e.Produto).ThenInclude(p => p.Categoria)
            .Include(e => e.Filial)
            .FirstOrDefaultAsync(e => e.FilialId == filialId && e.ProdutoId == produtoId);

    public async Task<IEnumerable<EstoqueFilial>> GetByFilialAsync(int filialId) =>
        await _ctx.EstoqueFiliais
            .Include(e => e.Produto).ThenInclude(p => p.Categoria)
            .Where(e => e.FilialId == filialId)
            .OrderBy(e => e.Produto.Nome)
            .ToListAsync();

    public async Task<IEnumerable<EstoqueFilial>> GetAbaixoMinimoAsync(int filialId) =>
        await _ctx.EstoqueFiliais
            .Include(e => e.Produto)
            .Where(e => e.FilialId == filialId && e.QuantidadeAtual <= e.Produto.EstoqueMinimo)
            .ToListAsync();

    public async Task AddOrUpdateEstoqueAsync(EstoqueFilial estoque)
    {
        var existente = await _ctx.EstoqueFiliais
            .FirstOrDefaultAsync(e => e.FilialId == estoque.FilialId && e.ProdutoId == estoque.ProdutoId);

        if (existente == null)
            _ctx.EstoqueFiliais.Add(estoque);
        else
        {
            existente.QuantidadeAtual = estoque.QuantidadeAtual;
            existente.DataValidade = estoque.DataValidade;
            _ctx.EstoqueFiliais.Update(existente);
        }

        await _ctx.SaveChangesAsync();
    }

    public async Task AddMovimentacaoAsync(MovimentacaoEstoque movimentacao)
    {
        _ctx.MovimentacoesEstoque.Add(movimentacao);
        await _ctx.SaveChangesAsync();
    }

    public async Task<IEnumerable<MovimentacaoEstoque>> GetMovimentacoesAsync(int filialId, int produtoId) =>
        await _ctx.MovimentacoesEstoque
            .Include(m => m.Produto)
            .Include(m => m.Funcionario)
            .Where(m => m.FilialId == filialId && m.ProdutoId == produtoId)
            .OrderByDescending(m => m.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<MovimentacaoEstoque>> GetMovimentacoesRecentesAsync(int produtoId, int diasHistorico)
    {
        var limite = DateTime.Now.AddDays(-diasHistorico);
        return await _ctx.MovimentacoesEstoque
            .Where(m => m.ProdutoId == produtoId && m.DataHora >= limite)
            .ToListAsync();
    }

    public async Task AddAlertaAsync(AlertaReposicao alerta)
    {
        var alertaExistente = await _ctx.AlertasReposicao
            .FirstOrDefaultAsync(a => a.FilialId == alerta.FilialId && a.ProdutoId == alerta.ProdutoId && !a.Resolvido);

        if (alertaExistente == null)
        {
            _ctx.AlertasReposicao.Add(alerta);
            await _ctx.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<AlertaReposicao>> GetAlertasAbertosAsync(int filialId) =>
        await _ctx.AlertasReposicao
            .Include(a => a.Produto)
            .Include(a => a.Filial)
            .Where(a => a.FilialId == filialId && !a.Resolvido)
            .OrderByDescending(a => a.DataAlerta)
            .ToListAsync();

    public async Task ResolverAlertaAsync(int alertaId)
    {
        var alerta = await _ctx.AlertasReposicao.FindAsync(alertaId);
        if (alerta != null)
        {
            alerta.Resolvido = true;
            await _ctx.SaveChangesAsync();
        }
    }
}
