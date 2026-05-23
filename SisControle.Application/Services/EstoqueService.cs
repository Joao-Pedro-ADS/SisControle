using SisControle.Domain.Entities;
using SisControle.Domain.Enums;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class EstoqueService
{
    private readonly IEstoqueRepository _estoqueRepo;
    private readonly IProdutoRepository _produtoRepo;

    public EstoqueService(IEstoqueRepository estoqueRepo, IProdutoRepository produtoRepo)
    {
        _estoqueRepo = estoqueRepo;
        _produtoRepo = produtoRepo;
    }

    public Task<IEnumerable<EstoqueFilial>> ListarPorFilialAsync(int filialId) =>
        _estoqueRepo.GetByFilialAsync(filialId);

    public Task<IEnumerable<AlertaReposicao>> ListarAlertasAsync(int filialId) =>
        _estoqueRepo.GetAlertasAbertosAsync(filialId);

    public Task ResolverAlertaAsync(int alertaId) =>
        _estoqueRepo.ResolverAlertaAsync(alertaId);

    public async Task RegistrarEntradaAsync(int filialId, int produtoId, int quantidade, string observacao, int? funcionarioId = null)
    {
        // Regra 1 — Toda movimentação precisa de origem identificada
        if (quantidade <= 0)
            throw new InvalidOperationException("Quantidade de entrada deve ser maior que zero.");

        var produto = await _produtoRepo.GetByIdAsync(produtoId)
            ?? throw new InvalidOperationException("Produto não encontrado.");

        var estoque = await _estoqueRepo.GetEstoqueAsync(filialId, produtoId)
            ?? new EstoqueFilial { FilialId = filialId, ProdutoId = produtoId, QuantidadeAtual = 0 };

        estoque.QuantidadeAtual += quantidade;
        await _estoqueRepo.AddOrUpdateEstoqueAsync(estoque);

        await _estoqueRepo.AddMovimentacaoAsync(new MovimentacaoEstoque
        {
            FilialId = filialId,
            ProdutoId = produtoId,
            Tipo = TipoMovimentacao.Entrada,
            Quantidade = quantidade,
            Observacao = observacao,
            FuncionarioId = funcionarioId
        });
    }

    public async Task RegistrarSaidaAsync(int filialId, int produtoId, int quantidade, TipoMovimentacao tipo, string observacao, int? funcionarioId = null)
    {
        // Regra 1 — Tipo obrigatório e restrito ao enum
        if (tipo == TipoMovimentacao.Entrada || tipo == TipoMovimentacao.Transferencia)
            throw new InvalidOperationException("Use o método correto para entrada ou transferência.");

        var estoque = await _estoqueRepo.GetEstoqueAsync(filialId, produtoId)
            ?? throw new InvalidOperationException("Produto sem estoque nesta filial.");

        // Regra 2 — O estoque nunca pode ficar negativo
        if (estoque.QuantidadeAtual < quantidade)
            throw new InvalidOperationException($"Estoque insuficiente. Disponível: {estoque.QuantidadeAtual}, solicitado: {quantidade}.");

        // Regra 5 — Produtos vencidos não podem ser vendidos (somente via Vencimento)
        if (tipo == TipoMovimentacao.Saida && estoque.DataValidade.HasValue && estoque.DataValidade.Value < DateTime.Today)
            throw new InvalidOperationException("Produto vencido. Registre como perda por vencimento.");

        estoque.QuantidadeAtual -= quantidade;
        await _estoqueRepo.AddOrUpdateEstoqueAsync(estoque);

        await _estoqueRepo.AddMovimentacaoAsync(new MovimentacaoEstoque
        {
            FilialId = filialId,
            ProdutoId = produtoId,
            Tipo = tipo,
            Quantidade = quantidade,
            Observacao = observacao,
            FuncionarioId = funcionarioId
        });

        // Regra 3 — Alerta automático de reposição
        var produto = await _produtoRepo.GetByIdAsync(produtoId);
        if (produto != null && estoque.QuantidadeAtual <= produto.EstoqueMinimo)
        {
            await _estoqueRepo.AddAlertaAsync(new AlertaReposicao
            {
                FilialId = filialId,
                ProdutoId = produtoId,
                QuantidadeAtual = estoque.QuantidadeAtual,
                EstoqueMinimo = produto.EstoqueMinimo
            });
        }
    }

    public async Task TransferirAsync(int filialOrigemId, int filialDestinoId, int produtoId, int quantidade, int? funcionarioId = null)
    {
        // Regra 4 — Transferência exige consistência nas duas pontas (operação atômica)
        if (filialOrigemId == filialDestinoId)
            throw new InvalidOperationException("Origem e destino da transferência não podem ser a mesma filial.");

        var estoqueOrigem = await _estoqueRepo.GetEstoqueAsync(filialOrigemId, produtoId)
            ?? throw new InvalidOperationException("Produto sem estoque na filial de origem.");

        // Regra 2 aplicada na transferência
        if (estoqueOrigem.QuantidadeAtual < quantidade)
            throw new InvalidOperationException($"Estoque insuficiente na origem. Disponível: {estoqueOrigem.QuantidadeAtual}.");

        var estoqueDestino = await _estoqueRepo.GetEstoqueAsync(filialDestinoId, produtoId)
            ?? new EstoqueFilial { FilialId = filialDestinoId, ProdutoId = produtoId, QuantidadeAtual = 0 };

        estoqueOrigem.QuantidadeAtual -= quantidade;
        estoqueDestino.QuantidadeAtual += quantidade;

        await _estoqueRepo.AddOrUpdateEstoqueAsync(estoqueOrigem);
        await _estoqueRepo.AddOrUpdateEstoqueAsync(estoqueDestino);

        var obs = $"Transferência para filial {filialDestinoId}";
        await _estoqueRepo.AddMovimentacaoAsync(new MovimentacaoEstoque
        {
            FilialId = filialOrigemId,
            ProdutoId = produtoId,
            Tipo = TipoMovimentacao.Transferencia,
            Quantidade = quantidade,
            Observacao = obs,
            FuncionarioId = funcionarioId
        });

        await _estoqueRepo.AddMovimentacaoAsync(new MovimentacaoEstoque
        {
            FilialId = filialDestinoId,
            ProdutoId = produtoId,
            Tipo = TipoMovimentacao.Entrada,
            Quantidade = quantidade,
            Observacao = $"Transferência recebida da filial {filialOrigemId}",
            FuncionarioId = funcionarioId
        });

        // Regra 3 — checar alerta na origem após transferência
        var produto = await _produtoRepo.GetByIdAsync(produtoId);
        if (produto != null && estoqueOrigem.QuantidadeAtual <= produto.EstoqueMinimo)
        {
            await _estoqueRepo.AddAlertaAsync(new AlertaReposicao
            {
                FilialId = filialOrigemId,
                ProdutoId = produtoId,
                QuantidadeAtual = estoqueOrigem.QuantidadeAtual,
                EstoqueMinimo = produto.EstoqueMinimo
            });
        }
    }

    public async Task BaixarEstoquePorVendaAsync(int filialId, int produtoId, int quantidade)
    {
        var estoque = await _estoqueRepo.GetEstoqueAsync(filialId, produtoId)
            ?? throw new InvalidOperationException($"Produto {produtoId} sem estoque na filial {filialId}.");

        if (estoque.QuantidadeAtual < quantidade)
            throw new InvalidOperationException($"Estoque insuficiente. Disponível: {estoque.QuantidadeAtual}.");

        estoque.QuantidadeAtual -= quantidade;
        await _estoqueRepo.AddOrUpdateEstoqueAsync(estoque);

        await _estoqueRepo.AddMovimentacaoAsync(new MovimentacaoEstoque
        {
            FilialId = filialId,
            ProdutoId = produtoId,
            Tipo = TipoMovimentacao.Saida,
            Quantidade = quantidade,
            Observacao = "Baixa automática por venda"
        });

        var produto = await _produtoRepo.GetByIdAsync(produtoId);
        if (produto != null && estoque.QuantidadeAtual <= produto.EstoqueMinimo)
        {
            await _estoqueRepo.AddAlertaAsync(new AlertaReposicao
            {
                FilialId = filialId,
                ProdutoId = produtoId,
                QuantidadeAtual = estoque.QuantidadeAtual,
                EstoqueMinimo = produto.EstoqueMinimo
            });
        }
    }

    public async Task ReintegrarEstoquePorDevolucaoAsync(int filialId, int produtoId, int quantidade)
    {
        var estoque = await _estoqueRepo.GetEstoqueAsync(filialId, produtoId)
            ?? new EstoqueFilial { FilialId = filialId, ProdutoId = produtoId, QuantidadeAtual = 0 };

        estoque.QuantidadeAtual += quantidade;
        await _estoqueRepo.AddOrUpdateEstoqueAsync(estoque);

        await _estoqueRepo.AddMovimentacaoAsync(new MovimentacaoEstoque
        {
            FilialId = filialId,
            ProdutoId = produtoId,
            Tipo = TipoMovimentacao.Devolucao,
            Quantidade = quantidade,
            Observacao = "Reintegração por devolução"
        });
    }

    // Regra 6 — Previsão de demanda respeita histórico mínimo de 30 dias
    public async Task<double?> PreverDemandaDiariaAsync(int produtoId)
    {
        var historico = (await _estoqueRepo.GetMovimentacoesRecentesAsync(produtoId, 30)).ToList();
        if (!historico.Any()) return null;

        var primeiraMov = historico.Min(m => m.DataHora);
        var diasComHistorico = (DateTime.Now - primeiraMov).TotalDays;
        if (diasComHistorico < 30) return null;

        var totalSaidas = historico
            .Where(m => m.Tipo == TipoMovimentacao.Saida || m.Tipo == TipoMovimentacao.Devolucao)
            .Sum(m => m.Quantidade);

        return totalSaidas / diasComHistorico;
    }
}
