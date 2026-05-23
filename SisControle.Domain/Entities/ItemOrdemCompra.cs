namespace SisControle.Domain.Entities;

public class ItemOrdemCompra
{
    public int Id { get; set; }
    public int OrdemCompraId { get; set; }
    public OrdemCompra OrdemCompra { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}
