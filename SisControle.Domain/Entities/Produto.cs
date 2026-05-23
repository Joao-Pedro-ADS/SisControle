namespace SisControle.Domain.Entities;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
    public decimal PrecoCusto { get; set; }
    public int EstoqueMinimo { get; set; }
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public ICollection<ItemVenda> ItensVenda { get; set; } = new List<ItemVenda>();
    public ICollection<ItemOrdemCompra> ItensOrdemCompra { get; set; } = new List<ItemOrdemCompra>();
    public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();
    public ICollection<EstoqueFilial> EstoqueFiliais { get; set; } = new List<EstoqueFilial>();
}
