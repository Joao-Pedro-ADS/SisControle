using SisControle.Domain.Entities;
using SisControle.Domain.Enums;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class OrdemCompraService
{
    private readonly IOrdemCompraRepository _repo;
    private readonly EstoqueService _estoqueService;

    public OrdemCompraService(IOrdemCompraRepository repo, EstoqueService estoqueService)
    {
        _repo = repo;
        _estoqueService = estoqueService;
    }

    public Task<IEnumerable<OrdemCompra>> ListarPorFilialAsync(int filialId) =>
        _repo.GetByFilialAsync(filialId);

    public Task<OrdemCompra?> ObterAsync(int id) => _repo.GetByIdAsync(id);

    public async Task CriarAsync(OrdemCompra ordem)
    {
        if (ordem.FilialId <= 0) throw new InvalidOperationException("Filial obrigatória.");
        if (ordem.FornecedorId <= 0) throw new InvalidOperationException("Fornecedor obrigatório.");
        if (!ordem.Itens.Any()) throw new InvalidOperationException("A ordem deve ter pelo menos um item.");

        ordem.Status = StatusOrdemCompra.Pendente;
        ordem.DataPedido = DateTime.Now;
        ordem.ValorTotal = ordem.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

        await _repo.AddAsync(ordem);
    }

    public async Task ReceberAsync(int ordemId)
    {
        var ordem = await _repo.GetByIdAsync(ordemId)
            ?? throw new InvalidOperationException("Ordem de compra não encontrada.");

        if (ordem.Status != StatusOrdemCompra.Aprovada && ordem.Status != StatusOrdemCompra.Pendente)
            throw new InvalidOperationException("Apenas ordens pendentes ou aprovadas podem ser recebidas.");

        foreach (var item in ordem.Itens)
            await _estoqueService.RegistrarEntradaAsync(
                ordem.FilialId, item.ProdutoId, item.Quantidade,
                $"Recebimento da ordem de compra #{ordemId}");

        ordem.Status = StatusOrdemCompra.Recebida;
        ordem.DataRecebimento = DateTime.Now;
        await _repo.UpdateAsync(ordem);
    }

    public async Task AtualizarStatusAsync(int ordemId, StatusOrdemCompra novoStatus)
    {
        var ordem = await _repo.GetByIdAsync(ordemId)
            ?? throw new InvalidOperationException("Ordem não encontrada.");
        ordem.Status = novoStatus;
        await _repo.UpdateAsync(ordem);
    }
}
