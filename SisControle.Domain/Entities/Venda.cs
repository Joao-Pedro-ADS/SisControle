namespace SisControle.Domain.Entities;

public class Venda
{
    public int Id { get; set; }
    public int FilialId { get; set; }
    public Filial Filial { get; set; } = null!;
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int FuncionarioId { get; set; }
    public Funcionario Funcionario { get; set; } = null!;
    public DateTime DataHora { get; set; } = DateTime.Now;
    public decimal ValorTotal { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;

    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}
