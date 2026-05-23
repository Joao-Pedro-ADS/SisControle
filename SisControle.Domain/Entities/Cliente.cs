namespace SisControle.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
}
