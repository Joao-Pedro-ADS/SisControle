using SisControle.Domain.Enums;

namespace SisControle.Domain.Entities;

public class DespesaOperacional
{
    public int Id { get; set; }
    public int FilialId { get; set; }
    public Filial Filial { get; set; } = null!;
    public TipoDespesa Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataLancamento { get; set; } = DateTime.Now;
    public string Competencia { get; set; } = string.Empty;
}
