using SisControle.Domain.Enums;

namespace SisControle.Domain.Entities;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public int FilialId { get; set; }
    public Filial Filial { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public TipoMovimentacao Tipo { get; set; }
    public int Quantidade { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
    public string Observacao { get; set; } = string.Empty;
    public int? FuncionarioId { get; set; }
    public Funcionario? Funcionario { get; set; }
}
