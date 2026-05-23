using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class VendaService
{
    private readonly IVendaRepository _vendaRepo;
    private readonly IEstoqueRepository _estoqueRepo;
    private readonly IProdutoRepository _produtoRepo;
    private readonly EstoqueService _estoqueService;

    public VendaService(
        IVendaRepository vendaRepo,
        IEstoqueRepository estoqueRepo,
        IProdutoRepository produtoRepo,
        EstoqueService estoqueService)
    {
        _vendaRepo = vendaRepo;
        _estoqueRepo = estoqueRepo;
        _produtoRepo = produtoRepo;
        _estoqueService = estoqueService;
    }

    public Task<IEnumerable<Venda>> ListarAsync() => _vendaRepo.GetAllAsync();
    public Task<Venda?> ObterAsync(int id) => _vendaRepo.GetByIdAsync(id);
    public Task<IEnumerable<Venda>> ListarPorFilialAsync(int filialId) => _vendaRepo.GetByFilialAsync(filialId);

    public async Task<Venda> RegistrarVendaAsync(Venda venda)
    {
        if (!venda.Itens.Any())
            throw new InvalidOperationException("A venda precisa ter pelo menos um item.");

        // Regra 7 — Verificar estoque de todos os itens antes de confirmar
        var bloqueados = new List<string>();
        foreach (var item in venda.Itens)
        {
            var estoque = await _estoqueRepo.GetEstoqueAsync(venda.FilialId, item.ProdutoId);
            if (estoque == null || estoque.QuantidadeAtual < item.Quantidade)
            {
                var produto = await _produtoRepo.GetByIdAsync(item.ProdutoId);
                var nomeProduto = produto?.Nome ?? $"ID {item.ProdutoId}";
                var disponivel = estoque?.QuantidadeAtual ?? 0;
                bloqueados.Add($"{nomeProduto} (disponível: {disponivel}, solicitado: {item.Quantidade})");
            }
        }

        if (bloqueados.Any())
            throw new InvalidOperationException(
                $"Venda bloqueada por estoque insuficiente: {string.Join("; ", bloqueados)}");

        // Regra 8 — O preço registrado é o preço vigente no momento da venda
        decimal valorTotal = 0;
        foreach (var item in venda.Itens)
        {
            var produto = await _produtoRepo.GetByIdAsync(item.ProdutoId)
                ?? throw new InvalidOperationException($"Produto {item.ProdutoId} não encontrado.");

            // Regra 5 — Produto vencido não pode ser vendido
            var estoque = await _estoqueRepo.GetEstoqueAsync(venda.FilialId, item.ProdutoId);
            if (estoque?.DataValidade.HasValue == true && estoque.DataValidade.Value < DateTime.Today)
                throw new InvalidOperationException($"Produto '{produto.Nome}' está vencido e não pode ser vendido.");

            item.PrecoUnitario = produto.PrecoVenda;
            valorTotal += item.PrecoUnitario * item.Quantidade;
        }

        venda.ValorTotal = valorTotal;
        venda.DataHora = DateTime.Now;

        // Regra 9 — Venda e baixa de estoque são a mesma transação
        await _vendaRepo.AddAsync(venda);

        foreach (var item in venda.Itens)
            await _estoqueService.BaixarEstoquePorVendaAsync(venda.FilialId, item.ProdutoId, item.Quantidade);

        return venda;
    }

    // Regra 10 — Devolução reintegra o produto ao estoque
    public async Task RegistrarDevolucaoAsync(int vendaId, IEnumerable<(int ProdutoId, int Quantidade)> itensDevolvidos)
    {
        var venda = await _vendaRepo.GetByIdAsync(vendaId)
            ?? throw new InvalidOperationException("Venda não encontrada.");

        foreach (var (produtoId, quantidade) in itensDevolvidos)
        {
            var item = venda.Itens.FirstOrDefault(i => i.ProdutoId == produtoId)
                ?? throw new InvalidOperationException($"Produto {produtoId} não encontrado na venda.");

            if (quantidade > item.Quantidade)
                throw new InvalidOperationException("Quantidade devolvida não pode ser maior que a vendida.");

            await _estoqueService.ReintegrarEstoquePorDevolucaoAsync(venda.FilialId, produtoId, quantidade);
        }
    }

    public Task<IEnumerable<Venda>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim) =>
        _vendaRepo.GetByPeriodoAsync(inicio, fim);

    public Task<IEnumerable<Venda>> ListarPorFilialEPeriodoAsync(int filialId, DateTime inicio, DateTime fim) =>
        _vendaRepo.GetByFilialEPeriodoAsync(filialId, inicio, fim);
}
