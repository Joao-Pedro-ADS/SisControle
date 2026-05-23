using SisControle.Domain.Entities;

namespace SisControle.Domain.Interfaces;

public interface IEstoqueRepository
{
    Task<EstoqueFilial?> GetEstoqueAsync(int filialId, int produtoId);
    Task<IEnumerable<EstoqueFilial>> GetByFilialAsync(int filialId);
    Task<IEnumerable<EstoqueFilial>> GetAbaixoMinimoAsync(int filialId);
    Task AddOrUpdateEstoqueAsync(EstoqueFilial estoque);
    Task AddMovimentacaoAsync(MovimentacaoEstoque movimentacao);
    Task<IEnumerable<MovimentacaoEstoque>> GetMovimentacoesAsync(int filialId, int produtoId);
    Task<IEnumerable<MovimentacaoEstoque>> GetMovimentacoesRecentesAsync(int produtoId, int diasHistorico);
    Task AddAlertaAsync(AlertaReposicao alerta);
    Task<IEnumerable<AlertaReposicao>> GetAlertasAbertosAsync(int filialId);
    Task ResolverAlertaAsync(int alertaId);
}
