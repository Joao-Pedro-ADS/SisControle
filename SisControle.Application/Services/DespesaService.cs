using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class DespesaService
{
    private readonly IDespesaRepository _repo;
    public DespesaService(IDespesaRepository repo) => _repo = repo;

    public Task<IEnumerable<DespesaOperacional>> ListarPorFilialAsync(int filialId) =>
        _repo.GetByFilialAsync(filialId);

    public async Task RegistrarAsync(DespesaOperacional despesa)
    {
        // Regra 11 — Despesa obrigatoriamente vinculada a uma filial
        if (despesa.FilialId <= 0)
            throw new InvalidOperationException("Despesa deve estar vinculada a uma filial.");

        if (despesa.Valor <= 0)
            throw new InvalidOperationException("Valor da despesa deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(despesa.Descricao))
            throw new InvalidOperationException("Descrição da despesa é obrigatória.");

        despesa.DataLancamento = DateTime.Now;
        await _repo.AddAsync(despesa);
    }

    public Task AtualizarAsync(DespesaOperacional despesa) => _repo.UpdateAsync(despesa);
    public Task ExcluirAsync(int id) => _repo.DeleteAsync(id);

    public Task<IEnumerable<DespesaOperacional>> ListarPorFilialEPeriodoAsync(int filialId, DateTime inicio, DateTime fim) =>
        _repo.GetByFilialEPeriodoAsync(filialId, inicio, fim);
}
