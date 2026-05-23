namespace SisControle.Domain.Entities;

public class EstoqueFilial
{
    public int Id { get; set; }
    public int FilialId { get; set; }
    public Filial Filial { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int QuantidadeAtual { get; set; }
    public DateTime? DataValidade { get; set; }
}
