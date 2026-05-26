using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;
using SisControle.Domain.Enums;

namespace SisControle.Infrastructure.Data;

/// <summary>
/// Popula o banco com dados realistas para testes de desempenho.
///
/// Volume gerado:
///   5  filiais  |  8  categorias  |  5  fornecedores
///  105 produtos |  150 clientes   |  25 funcionários (5/filial)
///  525 entradas de estoque        |  ~30 alertas de reposição
///  3 000 vendas | ~7 500 itens de venda
///  360 despesas operacionais (5 filiais × 12 meses × 6 tipos)
/// </summary>
public static class DbSeeder
{
    // Semente fixa → resultado reproduzível a cada reset do banco
    private static readonly Random Rng = new(42);

    // ── Pesos de vendas por filial (0-based index) ───────────────
    // SP=40 % · RJ=25 % · BH=15 % · PR=12 % · BA=8 %
    private static readonly int[] FilialCum = { 40, 65, 80, 92, 100 };

    private static readonly string[] FormasPagamento =
        { "PIX", "PIX", "PIX", "Cartão Débito", "Cartão Débito", "Cartão Crédito", "Dinheiro" };

    // ────────────────────────────────────────────────────────────
    public static async Task SeedAsync(AppDbContext ctx)
    {
        if (await ctx.Categorias.AnyAsync()) return;

        var swTotal = Stopwatch.StartNew();
        Console.WriteLine("[Seeder] ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("[Seeder] Iniciando seed do banco de dados...");
        Console.WriteLine("[Seeder] ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        var cats  = await TimedStep("Categorias    (8)",  () => StepCategoriasAsync(ctx));
        var fils  = await TimedStep("Filiais       (5)",  () => StepFiliaisAsync(ctx));
                    await TimedStep("Fornecedores  (5)",  () => StepFornecedoresAsync(ctx));
        var prods = await TimedStep("Produtos    (105)",  () => StepProdutosAsync(ctx, cats));
        var clis  = await TimedStep("Clientes    (150)",  () => StepClientesAsync(ctx));
        var funcs = await TimedStep("Funcionários (25)",  () => StepFuncionariosAsync(ctx, fils));
                    await TimedStep("Estoque     (525)",  () => StepEstoqueAsync(ctx, fils, prods));
                    await TimedStep("Vendas    (3 000)",  () => StepVendasAsync(ctx, fils, prods, clis, funcs));
                    await TimedStep("Despesas    (360)",  () => StepDespesasAsync(ctx, fils));

        swTotal.Stop();
        Console.WriteLine("[Seeder] ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine($"[Seeder] ✓ Seed concluído em {swTotal.Elapsed.TotalSeconds:F2}s  ({swTotal.Elapsed.TotalMilliseconds:F0}ms)");
        Console.WriteLine("[Seeder] ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    // ── Helpers de tempo ─────────────────────────────────────────
    private static async Task<T> TimedStep<T>(string label, Func<Task<T>> fn)
    {
        var sw = Stopwatch.StartNew();
        Console.Write($"[Seeder]   {label,-20} ... ");
        var result = await fn();
        sw.Stop();
        Console.WriteLine($"{sw.Elapsed.TotalMilliseconds,7:F0} ms");
        return result;
    }

    private static async Task TimedStep(string label, Func<Task> fn)
    {
        var sw = Stopwatch.StartNew();
        Console.Write($"[Seeder]   {label,-20} ... ");
        await fn();
        sw.Stop();
        Console.WriteLine($"{sw.Elapsed.TotalMilliseconds,7:F0} ms");
    }

    // ── Utilitários ──────────────────────────────────────────────
    private static T Pick<T>(IList<T> lst) => lst[Rng.Next(lst.Count)];
    private static T Pick<T>(T[] arr)      => arr[Rng.Next(arr.Length)];

    private static int WeightedFilialIdx()
    {
        var roll = Rng.Next(100);
        for (var i = 0; i < FilialCum.Length; i++)
            if (roll < FilialCum[i]) return i;
        return FilialCum.Length - 1;
    }

    // Datas distribuídas nos últimos 12 meses com pesos mensais
    private static readonly int[] MonthWeights = { 7,6,8,7,7,7,9,7,7,8,9,12 };
    private static readonly int[] MonthCum;
    static DbSeeder()
    {
        MonthCum = new int[12];
        MonthCum[0] = MonthWeights[0];
        for (var i = 1; i < 12; i++) MonthCum[i] = MonthCum[i - 1] + MonthWeights[i];
    }

    private static DateTime RandomDateUltimos12Meses()
    {
        var roll = Rng.Next(100);
        int mes  = 0;
        for (var i = 0; i < 12; i++) { if (roll < MonthCum[i]) { mes = i; break; } }
        var hoje    = DateTime.Today;
        var inicio  = hoje.AddMonths(-11).AddDays(-hoje.Day + 1); // início do mês há 11 meses
        var baseRef = inicio.AddMonths(mes);
        var diasMes = DateTime.DaysInMonth(baseRef.Year, baseRef.Month);
        return baseRef.AddDays(Rng.Next(diasMes))
                      .AddHours(Rng.Next(7, 22))
                      .AddMinutes(Rng.Next(60));
    }

    // ── 1. Categorias ────────────────────────────────────────────
    private static async Task<Categoria[]> StepCategoriasAsync(AppDbContext ctx)
    {
        var cats = new[]
        {
            new Categoria { Nome = "Alimentos",          Descricao = "Produtos alimentícios em geral" },
            new Categoria { Nome = "Bebidas",            Descricao = "Bebidas alcoólicas e não alcoólicas" },
            new Categoria { Nome = "Limpeza",            Descricao = "Produtos de limpeza doméstica" },
            new Categoria { Nome = "Higiene",            Descricao = "Produtos de higiene pessoal" },
            new Categoria { Nome = "Frios e Laticínios", Descricao = "Queijos, embutidos e laticínios" },
            new Categoria { Nome = "Hortifruti",         Descricao = "Frutas, legumes e verduras" },
            new Categoria { Nome = "Padaria",            Descricao = "Pães, bolos e confeitaria" },
            new Categoria { Nome = "Mercearia",          Descricao = "Grãos, cereais e especiarias" },
        };
        ctx.Categorias.AddRange(cats);
        await ctx.SaveChangesAsync();
        return cats;
    }

    // ── 2. Filiais ───────────────────────────────────────────────
    private static async Task<Filial[]> StepFiliaisAsync(AppDbContext ctx)
    {
        var fils = new[]
        {
            new Filial { Nome = "Filial São Paulo",      Cnpj = "11111111000101", Endereco = "Av. Paulista, 1000",       Cidade = "São Paulo",      Estado = "SP", Telefone = "(11) 3000-1001" },
            new Filial { Nome = "Filial Rio de Janeiro", Cnpj = "22222222000102", Endereco = "Rua das Laranjeiras, 200", Cidade = "Rio de Janeiro", Estado = "RJ", Telefone = "(21) 3000-1002" },
            new Filial { Nome = "Filial Belo Horizonte", Cnpj = "33333333000103", Endereco = "Av. Afonso Pena, 500",     Cidade = "Belo Horizonte", Estado = "MG", Telefone = "(31) 3000-1003" },
            new Filial { Nome = "Filial Curitiba",       Cnpj = "44444444000104", Endereco = "Rua XV de Novembro, 300",  Cidade = "Curitiba",       Estado = "PR", Telefone = "(41) 3000-1004" },
            new Filial { Nome = "Filial Salvador",       Cnpj = "55555555000105", Endereco = "Av. Tancredo Neves, 400",  Cidade = "Salvador",       Estado = "BA", Telefone = "(71) 3000-1005" },
        };
        ctx.Filiais.AddRange(fils);
        await ctx.SaveChangesAsync();
        return fils;
    }

    // ── 3. Fornecedores ──────────────────────────────────────────
    private static async Task StepFornecedoresAsync(AppDbContext ctx)
    {
        ctx.Fornecedores.AddRange(
            new Fornecedor { Nome = "Distribuidora Norte Sul",   Cnpj = "10000000000101", Telefone = "(11) 4000-0001", Email = "contato@nortesul.com.br",   Endereco = "Av. Brasil, 500 - SP"       },
            new Fornecedor { Nome = "Atacadão Central Ltda",     Cnpj = "20000000000102", Telefone = "(21) 4000-0002", Email = "vendas@atacadaocentral.com", Endereco = "Rua Comércio, 200 - RJ"     },
            new Fornecedor { Nome = "Frigorífico BH Alimentos",  Cnpj = "30000000000103", Telefone = "(31) 4000-0003", Email = "bh@frigorificobh.com.br",    Endereco = "Rod. BR-040, km 10 - MG"    },
            new Fornecedor { Nome = "Bebidas Sul Distribuidora", Cnpj = "40000000000104", Telefone = "(41) 4000-0004", Email = "sul@bebidasdist.com.br",     Endereco = "Av. Iguaçu, 800 - PR"       },
            new Fornecedor { Nome = "Tropical Foods Bahia",      Cnpj = "50000000000105", Telefone = "(71) 4000-0005", Email = "tropical@tropicalfoods.com", Endereco = "Av. ACM, 1200 - BA"         }
        );
        await ctx.SaveChangesAsync();
    }

    // ── 4. Produtos (105) ────────────────────────────────────────
    private static async Task<Produto[]> StepProdutosAsync(AppDbContext ctx, Categoria[] cats)
    {
        Produto P(string nome, string cod, decimal custo, decimal venda, int min, Categoria cat) =>
            new() { Nome = nome, CodigoBarras = cod, PrecoCusto = custo, PrecoVenda = venda, EstoqueMinimo = min, CategoriaId = cat.Id };

        var (ali, beb, lim, hig, fri, hor, pad, mer) =
            (cats[0], cats[1], cats[2], cats[3], cats[4], cats[5], cats[6], cats[7]);

        var prods = new[]
        {
            // Alimentos (15)
            P("Arroz Agulhinha 1kg",        "7891000101",  3.50m,  5.99m, 30, ali),
            P("Feijão Carioca 1kg",         "7891000102",  5.20m,  8.49m, 25, ali),
            P("Açúcar Cristal 1kg",         "7891000103",  2.80m,  4.99m, 30, ali),
            P("Farinha de Trigo 1kg",       "7891000104",  3.10m,  5.49m, 20, ali),
            P("Óleo de Soja 900ml",         "7891000105",  6.50m, 10.99m, 25, ali),
            P("Macarrão Espaguete 500g",    "7891000106",  2.40m,  3.99m, 35, ali),
            P("Molho de Tomate 340g",       "7891000107",  1.80m,  2.99m, 40, ali),
            P("Sal Refinado 1kg",           "7891000108",  1.20m,  1.99m, 20, ali),
            P("Tempero Completo 300g",      "7891000109",  4.50m,  7.49m, 15, ali),
            P("Caldo de Carne 57g",         "7891000110",  2.10m,  3.49m, 30, ali),
            P("Vinagre 750ml",              "7891000111",  1.60m,  2.79m, 20, ali),
            P("Extrato de Tomate 200g",     "7891000112",  1.50m,  2.49m, 25, ali),
            P("Leite em Pó 400g",           "7891000113",  8.90m, 14.99m, 15, ali),
            P("Achocolatado 400g",          "7891000114",  6.40m, 10.49m, 20, ali),
            P("Granola Tradicional 500g",   "7891000115",  5.80m,  9.49m, 15, ali),

            // Bebidas (15)
            P("Água Mineral 500ml",         "7891000201",  1.00m,  1.99m, 50, beb),
            P("Água Mineral 1,5L",          "7891000202",  1.80m,  3.29m, 40, beb),
            P("Refrigerante Cola 2L",       "7891000203",  5.50m,  8.99m, 30, beb),
            P("Refrigerante Guaraná 2L",    "7891000204",  5.20m,  8.49m, 30, beb),
            P("Suco de Laranja 1L",         "7891000205",  5.80m,  9.99m, 20, beb),
            P("Suco de Uva 1L",             "7891000206",  6.20m, 10.49m, 20, beb),
            P("Cerveja Lata 350ml",         "7891000207",  2.80m,  4.99m, 60, beb),
            P("Cerveja Long Neck 600ml",    "7891000208",  5.00m,  8.49m, 30, beb),
            P("Vinho Tinto 750ml",          "7891000209", 14.00m, 24.99m, 15, beb),
            P("Energético 250ml",           "7891000210",  4.50m,  7.99m, 25, beb),
            P("Isotônico 500ml",            "7891000211",  2.80m,  4.99m, 20, beb),
            P("Chá Gelado 1,5L",            "7891000212",  4.20m,  6.99m, 20, beb),
            P("Leite Integral 1L",          "7891000213",  3.80m,  5.99m, 40, beb),
            P("Leite Desnatado 1L",         "7891000214",  4.10m,  6.49m, 30, beb),
            P("Suco Caixinha 200ml",        "7891000215",  1.20m,  1.99m, 40, beb),

            // Limpeza (13)
            P("Detergente Líquido 500ml",   "7891000301",  1.50m,  2.79m, 40, lim),
            P("Sabão em Pó 1kg",            "7891000302",  7.50m, 12.99m, 20, lim),
            P("Amaciante 2L",               "7891000303",  8.00m, 13.99m, 15, lim),
            P("Desinfetante Pinho 1L",      "7891000304",  3.50m,  5.99m, 20, lim),
            P("Água Sanitária 1L",          "7891000305",  2.80m,  4.49m, 20, lim),
            P("Esponja Dupla Face",         "7891000306",  0.90m,  1.69m, 50, lim),
            P("Sabão em Barra 5un",         "7891000307",  4.20m,  6.99m, 25, lim),
            P("Multiuso 500ml",             "7891000308",  4.80m,  7.99m, 15, lim),
            P("Limpa Vidros 500ml",         "7891000309",  4.50m,  7.49m, 15, lim),
            P("Inseticida 300ml",           "7891000310",  8.50m, 14.49m, 10, lim),
            P("Flanela Microfibra",         "7891000311",  3.50m,  5.99m, 15, lim),
            P("Rodo 60cm",                  "7891000312",  7.00m, 12.99m, 10, lim),
            P("Vassoura de Pelo",           "7891000313", 10.00m, 17.99m,  8, lim),

            // Higiene (12)
            P("Shampoo 400ml",              "7891000401",  8.50m, 14.99m, 15, hig),
            P("Condicionador 400ml",        "7891000402",  8.50m, 14.99m, 15, hig),
            P("Sabonete em Barra",          "7891000403",  1.50m,  2.49m, 40, hig),
            P("Sabonete Líquido 250ml",     "7891000404",  4.50m,  7.99m, 20, hig),
            P("Pasta de Dente 90g",         "7891000405",  3.20m,  5.49m, 30, hig),
            P("Fio Dental 50m",             "7891000406",  2.80m,  4.99m, 20, hig),
            P("Desodorante Spray 150ml",    "7891000407",  5.50m,  9.49m, 20, hig),
            P("Desodorante Roll-On 50ml",   "7891000408",  5.00m,  8.99m, 20, hig),
            P("Aparelho de Barbear 2un",    "7891000409",  3.50m,  5.99m, 15, hig),
            P("Absorvente 8un",             "7891000410",  4.20m,  6.99m, 20, hig),
            P("Papel Higiênico 4 Rolos",    "7891000411",  5.50m,  8.99m, 30, hig),
            P("Lenço de Papel 50un",        "7891000412",  2.50m,  3.99m, 20, hig),

            // Frios e Laticínios (12)
            P("Queijo Mussarela 500g",      "7891000501", 12.50m, 22.99m, 15, fri),
            P("Queijo Prato 300g",          "7891000502", 10.00m, 17.99m, 12, fri),
            P("Presunto Fatiado 200g",      "7891000503",  6.50m, 11.49m, 15, fri),
            P("Mortadela 300g",             "7891000504",  5.50m,  9.49m, 15, fri),
            P("Salsicha 500g",              "7891000505",  8.00m, 13.99m, 12, fri),
            P("Iogurte Natural 170g",       "7891000506",  1.80m,  2.99m, 30, fri),
            P("Iogurte Grego 100g",         "7891000507",  2.50m,  4.49m, 25, fri),
            P("Manteiga 200g",              "7891000508",  6.50m, 11.49m, 15, fri),
            P("Margarina 500g",             "7891000509",  5.80m,  9.99m, 15, fri),
            P("Creme de Leite 200g",        "7891000510",  2.80m,  4.99m, 20, fri),
            P("Requeijão 200g",             "7891000511",  4.50m,  7.99m, 15, fri),
            P("Cream Cheese 150g",          "7891000512",  5.80m,  9.99m, 12, fri),

            // Hortifruti (13)
            P("Banana Prata 1kg",           "7891000601",  2.50m,  4.49m, 20, hor),
            P("Maçã Fuji 1kg",              "7891000602",  5.00m,  8.99m, 15, hor),
            P("Laranja Pera 1kg",           "7891000603",  3.50m,  5.99m, 20, hor),
            P("Tomate Salada 1kg",          "7891000604",  4.00m,  6.99m, 20, hor),
            P("Batata Inglesa 1kg",         "7891000605",  3.80m,  6.49m, 20, hor),
            P("Cebola 1kg",                 "7891000606",  3.00m,  4.99m, 15, hor),
            P("Alho 250g",                  "7891000607",  4.50m,  7.99m, 10, hor),
            P("Cenoura 1kg",                "7891000608",  3.20m,  5.49m, 15, hor),
            P("Alface Americana",           "7891000609",  1.80m,  2.99m, 15, hor),
            P("Brócolis Maço",              "7891000610",  2.50m,  4.49m, 10, hor),
            P("Pepino Japonês",             "7891000611",  2.00m,  3.49m, 10, hor),
            P("Pimentão Verde",             "7891000612",  3.50m,  5.99m, 10, hor),
            P("Uva Thompson 1kg",           "7891000613",  8.50m, 14.99m, 10, hor),

            // Padaria (12)
            P("Pão de Forma 500g",          "7891000701",  5.50m,  8.99m, 20, pad),
            P("Bolo de Cenoura 400g",       "7891000702",  9.50m, 16.99m, 10, pad),
            P("Bolo de Chocolate 400g",     "7891000703",  9.50m, 16.99m, 10, pad),
            P("Biscoito Cream Cracker",     "7891000704",  3.20m,  5.49m, 20, pad),
            P("Biscoito Maisena 200g",      "7891000705",  3.00m,  4.99m, 20, pad),
            P("Biscoito Recheado 130g",     "7891000706",  2.80m,  4.49m, 25, pad),
            P("Wafer Baunilha 100g",        "7891000707",  2.50m,  3.99m, 20, pad),
            P("Rosquinha de Coco 300g",     "7891000708",  4.50m,  7.49m, 15, pad),
            P("Pão de Queijo 400g",         "7891000709",  7.50m, 12.99m, 15, pad),
            P("Cookie Chocolate 150g",      "7891000710",  5.50m,  8.99m, 15, pad),
            P("Torrada Integral 150g",      "7891000711",  4.20m,  6.99m, 15, pad),
            P("Croissant Congelado 300g",   "7891000712",  8.00m, 13.99m, 10, pad),

            // Mercearia (13)
            P("Arroz Integral 1kg",         "7891000801",  4.80m,  7.99m, 20, mer),
            P("Quinoa 250g",                "7891000802", 10.00m, 17.99m, 10, mer),
            P("Aveia em Flocos 500g",       "7891000803",  4.50m,  7.99m, 15, mer),
            P("Lentilha 500g",              "7891000804",  5.50m,  8.99m, 12, mer),
            P("Grão de Bico 500g",          "7891000805",  5.80m,  9.49m, 12, mer),
            P("Amendoim Torrado 500g",      "7891000806",  4.20m,  6.99m, 15, mer),
            P("Passas Escuras 200g",        "7891000807",  5.50m,  8.99m, 10, mer),
            P("Castanha de Caju 100g",      "7891000808",  8.00m, 14.99m,  8, mer),
            P("Mel Puro 500g",              "7891000809", 12.00m, 21.99m,  8, mer),
            P("Azeite Extra Virgem 250ml",  "7891000810", 14.00m, 24.99m, 10, mer),
            P("Vinagre Balsâmico 250ml",    "7891000811",  9.00m, 15.99m,  8, mer),
            P("Granola Premium 500g",       "7891000812",  9.50m, 16.99m, 10, mer),
            P("Chia 200g",                  "7891000813",  7.50m, 12.99m, 10, mer),
        };

        ctx.Produtos.AddRange(prods);
        await ctx.SaveChangesAsync();
        return prods;
    }

    // ── 5. Clientes (150) ────────────────────────────────────────
    private static async Task<Cliente[]> StepClientesAsync(AppDbContext ctx)
    {
        var prenomes  = new[] { "Ana","Bruno","Carlos","Diana","Eduardo","Fernanda","Gabriel","Helena","Igor","Juliana",
                                "Kevin","Larissa","Marcos","Natália","Oscar","Patrícia","Rodrigo","Sandra","Thiago","Vanessa",
                                "André","Beatriz","Cláudio","Denise","Fábio","Gisele","Henrique","Isabela","José","Kelly" };
        var sobrenomes = new[] { "Silva","Santos","Oliveira","Souza","Rodrigues","Ferreira","Alves","Pereira","Lima","Gomes",
                                 "Costa","Ribeiro","Martins","Carvalho","Almeida","Lopes","Sousa","Fernandes","Vieira","Barbosa" };
        var ddds = new[] { "11","21","31","41","71","51","61","85","92","47" };

        var clientes = Enumerable.Range(1, 150).Select(i =>
        {
            var pren = prenomes[(i - 1) % prenomes.Length];
            var sob  = sobrenomes[(i - 1) % sobrenomes.Length];
            var ddd  = Pick(ddds);
            return new Cliente
            {
                Nome     = $"{pren} {sob} {i}",
                Cpf      = $"{i / 10000:000}.{(i / 100) % 100:000}.{i % 100:000}-{i % 97:00}",
                Telefone = $"({ddd}) 9{Rng.Next(1000, 9999)}-{Rng.Next(1000, 9999)}",
                Email    = $"{pren.ToLower()}{sob.ToLower()}{i}@email.com",
            };
        }).ToArray();

        ctx.Clientes.AddRange(clientes);
        await ctx.SaveChangesAsync();
        return clientes;
    }

    // ── 6. Funcionários (5 por filial = 25) ──────────────────────
    private static async Task<Funcionario[]> StepFuncionariosAsync(AppDbContext ctx, Filial[] fils)
    {
        var nomes  = new[] { "João","Maria","Pedro","Carla","Lucas","Amanda","Rafael","Priscila","Diego","Aline",
                             "Felipe","Camila","Gustavo","Letícia","Renato","Tatiane","Sérgio","Mônica","Leandro","Roberta",
                             "Murilo","Juliana","Cleber","Simone","Vagner" };
        var cargos = new[] { "Gerente","Supervisor","Caixa","Repositor","Atendente" };

        var funcs = fils.SelectMany((fil, fi) =>
            cargos.Select((cargo, ci) =>
            {
                var nome = nomes[fi * 5 + ci];
                return new Funcionario
                {
                    Nome         = nome,
                    Cpf          = $"{fi + 1:000}.{ci + 1:000}.{(fi * 5 + ci + 1):000}-{(fi + ci) % 97:00}",
                    Cargo        = cargo,
                    Telefone     = $"({11 + fi * 10}) 9{Rng.Next(1000, 9999)}-{Rng.Next(1000, 9999)}",
                    Email        = $"{nome.ToLower()}.f{fi + 1}@siscontrole.com",
                    Status       = StatusFuncionario.Ativo,
                    FilialId     = fil.Id,
                    DataAdmissao = DateTime.Today.AddMonths(-Rng.Next(6, 36)),
                };
            })
        ).ToArray();

        ctx.Funcionarios.AddRange(funcs);
        await ctx.SaveChangesAsync();
        return funcs;
    }

    // ── 7. EstoqueFilial + AlertaReposicao ────────────────────────
    private static async Task StepEstoqueAsync(AppDbContext ctx, Filial[] fils, Produto[] prods)
    {
        var estoques = new List<EstoqueFilial>(fils.Length * prods.Length);
        var alertas  = new List<AlertaReposicao>();

        foreach (var fil in fils)
        {
            foreach (var prod in prods)
            {
                // ~10 % dos produtos ficam abaixo do mínimo para gerar alertas
                var abaixo = Rng.Next(10) == 0;
                var qtd    = abaixo
                    ? Rng.Next(0, prod.EstoqueMinimo)
                    : Rng.Next(prod.EstoqueMinimo, prod.EstoqueMinimo * 4 + 1);

                estoques.Add(new EstoqueFilial
                {
                    FilialId        = fil.Id,
                    ProdutoId       = prod.Id,
                    QuantidadeAtual = qtd,
                    DataValidade    = prod.CategoriaId <= 5  // perecíveis
                        ? DateTime.Today.AddDays(Rng.Next(3, 180))
                        : null,
                });

                if (abaixo)
                    alertas.Add(new AlertaReposicao
                    {
                        FilialId       = fil.Id,
                        ProdutoId      = prod.Id,
                        QuantidadeAtual = qtd,
                        EstoqueMinimo   = prod.EstoqueMinimo,
                        DataAlerta      = DateTime.Now.AddHours(-Rng.Next(0, 72)),
                        Resolvido       = false,
                    });
            }
        }

        ctx.Set<EstoqueFilial>().AddRange(estoques);
        ctx.Set<AlertaReposicao>().AddRange(alertas);
        await ctx.SaveChangesAsync();
    }

    // ── 8. Vendas + ItensVenda (3 000 vendas ≈ 7 500 itens) ─────
    private static async Task StepVendasAsync(
        AppDbContext ctx,
        Filial[]     fils,
        Produto[]    prods,
        Cliente[]    clis,
        Funcionario[] funcs)
    {
        const int TotalVendas = 3_000;
        const int BatchSize   = 300;

        // índices de funcionários por filial para lookup rápido
        var funcsPorFilial = funcs.GroupBy(f => f.FilialId)
                                  .ToDictionary(g => g.Key, g => g.ToList());

        var swVendas = Stopwatch.StartNew();

        for (var batch = 0; batch < TotalVendas; batch += BatchSize)
        {
            var limite    = Math.Min(BatchSize, TotalVendas - batch);
            var batchNum  = batch / BatchSize + 1;
            var batchTotal= (int)Math.Ceiling((double)TotalVendas / BatchSize);
            var vendas    = new List<Venda>(limite);

            for (var v = 0; v < limite; v++)
            {
                var filIdx  = WeightedFilialIdx();
                var fil     = fils[filIdx];
                var funcsF  = funcsPorFilial[fil.Id];
                var func    = Pick(funcsF);
                var cliente = Rng.Next(4) > 0 ? Pick(clis) : null; // 75 % com cliente
                var nItens  = Rng.Next(1, 6); // 1–5 itens por venda

                var itens = Enumerable.Range(0, nItens)
                    .Select(_ =>
                    {
                        var prod = Pick(prods);
                        var qty  = Rng.Next(1, 8);
                        return new ItemVenda
                        {
                            ProdutoId      = prod.Id,
                            Quantidade     = qty,
                            PrecoUnitario  = prod.PrecoVenda,
                        };
                    }).ToList();

                var venda = new Venda
                {
                    FilialId       = fil.Id,
                    ClienteId      = cliente?.Id,
                    FuncionarioId  = func.Id,
                    DataHora       = RandomDateUltimos12Meses(),
                    FormaPagamento = Pick(FormasPagamento),
                    ValorTotal     = itens.Sum(i => i.PrecoUnitario * i.Quantidade),
                    Itens          = itens,
                };

                vendas.Add(venda);
            }

            var swBatch = Stopwatch.StartNew();
            ctx.Vendas.AddRange(vendas);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear(); // libera memória após cada batch
            swBatch.Stop();
            Console.WriteLine($"[Seeder]     batch {batchNum,2}/{batchTotal} ({batch + limite,5} vendas) — {swBatch.Elapsed.TotalMilliseconds:F0}ms  | acumulado: {swVendas.Elapsed.TotalSeconds:F1}s");
        }
    }

    // ── 9. Despesas Operacionais (360 = 5 filiais × 12 meses × 6 tipos) ──
    private static async Task StepDespesasAsync(AppDbContext ctx, Filial[] fils)
    {
        // Valores-base por tipo (variam ±15 % por filial/mês)
        var baseValores = new Dictionary<TipoDespesa, decimal>
        {
            { TipoDespesa.Aluguel,    8_000m },
            { TipoDespesa.Energia,    2_500m },
            { TipoDespesa.Salario,   35_000m },
            { TipoDespesa.Manutencao,  800m },
            { TipoDespesa.Agua,        400m },
            { TipoDespesa.Internet,    300m },
        };

        // Multiplicador de porte por filial (SP maior, BA menor)
        var porteFilial = new[] { 2.0m, 1.5m, 1.2m, 1.0m, 0.8m };

        var despesas = new List<DespesaOperacional>(fils.Length * 12 * 6);
        var hoje     = DateTime.Today;

        for (var fi = 0; fi < fils.Length; fi++)
        {
            var fil = fils[fi];

            for (var m = 11; m >= 0; m--)          // últimos 12 meses
            {
                var refDate = hoje.AddMonths(-m);
                var compet  = refDate.ToString("MM/yyyy");

                foreach (var (tipo, baseVal) in baseValores)
                {
                    var variacao = 1m + (decimal)(Rng.NextDouble() * 0.30 - 0.15); // ±15 %
                    var valor    = Math.Round(baseVal * porteFilial[fi] * variacao, 2);

                    despesas.Add(new DespesaOperacional
                    {
                        FilialId       = fil.Id,
                        Tipo           = tipo,
                        Descricao      = $"{tipo} — {fil.Nome} — {compet}",
                        Valor          = valor,
                        DataLancamento = new DateTime(refDate.Year, refDate.Month,
                                             Math.Min(5, DateTime.DaysInMonth(refDate.Year, refDate.Month))),
                        Competencia    = compet,
                    });
                }
            }
        }

        ctx.Set<DespesaOperacional>().AddRange(despesas);
        await ctx.SaveChangesAsync();
    }
}
