using SisControle.Domain.Enums;

namespace SisControle.Domain.Entities;

public class OrdemCompra
{
    public int Id { get; set; }
    public int FilialId { get; set; }
    public Filial Filial { get; set; } = null!;
    public int FornecedorId { get; set; }
    public Fornecedor Fornecedor { get; set; } = null!;
    public DateTime DataPedido { get; set; } = DateTime.Now;
    public DateTime? DataRecebimento { get; set; }
    public StatusOrdemCompra Status { get; set; } = StatusOrdemCompra.Pendente;
    public decimal ValorTotal { get; set; }

    public ICollection<ItemOrdemCompra> Itens { get; set; } = new List<ItemOrdemCompra>();
}
