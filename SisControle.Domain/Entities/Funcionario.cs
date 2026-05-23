using SisControle.Domain.Enums;

namespace SisControle.Domain.Entities;

public class Funcionario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public StatusFuncionario Status { get; set; } = StatusFuncionario.Ativo;
    public int FilialId { get; set; }
    public Filial Filial { get; set; } = null!;
    public DateTime DataAdmissao { get; set; } = DateTime.Today;

    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
}
