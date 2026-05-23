using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;

namespace SisControle.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext ctx)
    {
        if (await ctx.Categorias.AnyAsync()) return;

        var categorias = new[]
        {
            new Categoria { Nome = "Alimentos", Descricao = "Produtos alimentícios em geral" },
            new Categoria { Nome = "Bebidas", Descricao = "Bebidas alcoólicas e não alcoólicas" },
            new Categoria { Nome = "Limpeza", Descricao = "Produtos de limpeza doméstica" },
            new Categoria { Nome = "Higiene", Descricao = "Produtos de higiene pessoal" },
            new Categoria { Nome = "Frios e Laticínios", Descricao = "Queijos, embutidos, iogurtes" },
            new Categoria { Nome = "Hortifruti", Descricao = "Frutas, legumes e verduras" },
            new Categoria { Nome = "Padaria", Descricao = "Pães, bolos e confeitaria" },
            new Categoria { Nome = "Mercearia", Descricao = "Arroz, feijão, macarrão e similares" },
        };

        ctx.Categorias.AddRange(categorias);

        var filial = new Filial
        {
            Nome = "Filial Central",
            Cnpj = "00000000000100",
            Endereco = "Rua Principal, 100",
            Cidade = "São Paulo",
            Estado = "SP",
            Telefone = "(11) 9999-9999"
        };
        ctx.Filiais.Add(filial);

        await ctx.SaveChangesAsync();
    }
}
