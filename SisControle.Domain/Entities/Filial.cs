namespace SisControle.Domain.Entities;

public class Filial
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;

    public ICollection<Funcionario> Funcionarios { get; set; } = new List<Funcionario>();
    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    public ICollection<OrdemCompra> OrdensCompra { get; set; } = new List<OrdemCompra>();
    public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();
    public ICollection<DespesaOperacional> Despesas { get; set; } = new List<DespesaOperacional>();
    public ICollection<EstoqueFilial> EstoqueFiliais { get; set; } = new List<EstoqueFilial>();
}
