namespace SisControle.Domain.Entities;

public class AlertaReposicao
{
    public int Id { get; set; }
    public int FilialId { get; set; }
    public Filial Filial { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int QuantidadeAtual { get; set; }
    public int EstoqueMinimo { get; set; }
    public DateTime DataAlerta { get; set; } = DateTime.Now;
    public bool Resolvido { get; set; } = false;
}
