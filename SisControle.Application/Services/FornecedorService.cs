using SisControle.Domain.Entities;
using SisControle.Domain.Interfaces;

namespace SisControle.Application.Services;

public class FornecedorService
{
    private readonly IFornecedorRepository _repo;
    public FornecedorService(IFornecedorRepository repo) => _repo = repo;

    public Task<IEnumerable<Fornecedor>> ListarAsync() => _repo.GetAllAsync();
    public Task<Fornecedor?> ObterAsync(int id) => _repo.GetByIdAsync(id);

    public async Task CadastrarAsync(Fornecedor fornecedor)
    {
        // Regra 15 — Fornecedor precisa de CNPJ válido
        if (!CnpjValido(fornecedor.Cnpj))
            throw new InvalidOperationException("CNPJ inválido. Verifique o número e os dígitos verificadores.");

        if (await _repo.CnpjExisteAsync(fornecedor.Cnpj))
            throw new InvalidOperationException("Já existe um fornecedor com este CNPJ.");

        if (string.IsNullOrWhiteSpace(fornecedor.Nome))
            throw new InvalidOperationException("Nome do fornecedor é obrigatório.");

        await _repo.AddAsync(fornecedor);
    }

    public async Task AtualizarAsync(Fornecedor fornecedor)
    {
        if (!CnpjValido(fornecedor.Cnpj))
            throw new InvalidOperationException("CNPJ inválido.");

        if (await _repo.CnpjExisteAsync(fornecedor.Cnpj, fornecedor.Id))
            throw new InvalidOperationException("Já existe outro fornecedor com este CNPJ.");

        await _repo.UpdateAsync(fornecedor);
    }

    public Task ExcluirAsync(int id) => _repo.DeleteAsync(id);

    private static bool CnpjValido(string cnpj)
    {
        cnpj = new string(cnpj.Where(char.IsDigit).ToArray());
        if (cnpj.Length != 14) return false;
        if (cnpj.Distinct().Count() == 1) return false;

        int[] mult1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        int soma = cnpj.Take(12).Select((c, i) => (c - '0') * mult1[i]).Sum();
        int resto = soma % 11;
        int d1 = resto < 2 ? 0 : 11 - resto;
        if ((cnpj[12] - '0') != d1) return false;

        soma = cnpj.Take(13).Select((c, i) => (c - '0') * mult2[i]).Sum();
        resto = soma % 11;
        int d2 = resto < 2 ? 0 : 11 - resto;
        return (cnpj[13] - '0') == d2;
    }
}
