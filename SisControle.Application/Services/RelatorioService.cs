using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public record RelatorioFilialDto(
    int FilialId,
    string NomeFilial,
    string Estado,
    decimal TotalFaturado,
    decimal TotalDespesas,
    decimal MargemBruta,
    int TotalVendas,
    int ProdutosComAlerta
);

public record ProdutoMaisVendidoDto(
    int ProdutoId,
    string NomeProduto,
    int QuantidadeVendida,
    decimal ValorTotalVendido
);

public record PerdasVencimentoDto(
    int FilialId,
    string NomeFilial,
    int ProdutoId,
    string NomeProduto,
    int QuantidadePerdida,
    DateTime DataMovimentacao
);

public class RelatorioService
{
    private readonly IVendaRepository _vendaRepo;
    private readonly IDespesaRepository _despesaRepo;
    private readonly IFilialRepository _filialRepo;
    private readonly IEstoqueRepository _estoqueRepo;

    public RelatorioService(
        IVendaRepository vendaRepo,
        IDespesaRepository despesaRepo,
        IFilialRepository filialRepo,
        IEstoqueRepository estoqueRepo)
    {
        _vendaRepo = vendaRepo;
        _despesaRepo = despesaRepo;
        _filialRepo = filialRepo;
        _estoqueRepo = estoqueRepo;
    }

    // Regra 16 — Sempre agrega por filial antes de totalizar a rede
    // Regra 17 — Usa o mesmo período para todas as filiais
    public async Task<IEnumerable<RelatorioFilialDto>> GerarRelatorioRedeAsync(DateTime inicio, DateTime fim)
    {
        var filiais = (await _filialRepo.GetAllAsync()).ToList();
        var resultado = new List<RelatorioFilialDto>();

        foreach (var filial in filiais)
        {
            var vendas = (await _vendaRepo.GetByFilialEPeriodoAsync(filial.Id, inicio, fim)).ToList();
            var despesas = (await _despesaRepo.GetByFilialEPeriodoAsync(filial.Id, inicio, fim)).ToList();
            var alertas = (await _estoqueRepo.GetAlertasAbertosAsync(filial.Id)).ToList();

            decimal faturado = vendas.Sum(v => v.ValorTotal);
            decimal custoVendas = vendas
                .SelectMany(v => v.Itens)
                .Sum(i => i.Produto?.PrecoCusto * i.Quantidade ?? 0);
            decimal totalDespesas = despesas.Sum(d => d.Valor);

            // Regra 12 — Separar custo de compra do preço de venda para calcular margem bruta
            decimal margemBruta = faturado - custoVendas;

            resultado.Add(new RelatorioFilialDto(
                filial.Id,
                filial.Nome,
                filial.Estado,
                faturado,
                totalDespesas,
                margemBruta,
                vendas.Count,
                alertas.Count
            ));
        }

        return resultado;
    }

    public async Task<RelatorioFilialDto> GerarRelatorioFilialAsync(int filialId, DateTime inicio, DateTime fim)
    {
        var filial = await _filialRepo.GetByIdAsync(filialId)
            ?? throw new InvalidOperationException("Filial não encontrada.");

        var vendas = (await _vendaRepo.GetByFilialEPeriodoAsync(filialId, inicio, fim)).ToList();
        var despesas = (await _despesaRepo.GetByFilialEPeriodoAsync(filialId, inicio, fim)).ToList();
        var alertas = (await _estoqueRepo.GetAlertasAbertosAsync(filialId)).ToList();

        decimal faturado = vendas.Sum(v => v.ValorTotal);
        decimal custoVendas = vendas
            .SelectMany(v => v.Itens)
            .Sum(i => i.Produto?.PrecoCusto * i.Quantidade ?? 0);
        decimal totalDespesas = despesas.Sum(d => d.Valor);
        decimal margemBruta = faturado - custoVendas;

        return new RelatorioFilialDto(filialId, filial.Nome, filial.Estado, faturado, totalDespesas, margemBruta, vendas.Count, alertas.Count);
    }

    public async Task<IEnumerable<ProdutoMaisVendidoDto>> GerarProdutosMaisVendidosAsync(DateTime inicio, DateTime fim, int top = 10)
    {
        var vendas = (await _vendaRepo.GetByPeriodoAsync(inicio, fim)).ToList();

        return vendas
            .SelectMany(v => v.Itens)
            .GroupBy(i => new { i.ProdutoId, Nome = i.Produto?.Nome ?? $"ID {i.ProdutoId}" })
            .Select(g => new ProdutoMaisVendidoDto(
                g.Key.ProdutoId,
                g.Key.Nome,
                g.Sum(i => i.Quantidade),
                g.Sum(i => i.PrecoUnitario * i.Quantidade)
            ))
            .OrderByDescending(p => p.QuantidadeVendida)
            .Take(top);
    }
}
