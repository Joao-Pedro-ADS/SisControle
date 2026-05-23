using Microsoft.EntityFrameworkCore;
using SisControle.Domain.Entities;

namespace SisControle.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Filial> Filiais { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<EstoqueFilial> EstoqueFiliais { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<ItemVenda> ItensVenda { get; set; }
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
    public DbSet<OrdemCompra> OrdensCompra { get; set; }
    public DbSet<ItemOrdemCompra> ItensOrdemCompra { get; set; }
    public DbSet<DespesaOperacional> DespesasOperacionais { get; set; }
    public DbSet<AlertaReposicao> AlertasReposicao { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Produto>(e =>
        {
            e.Property(p => p.PrecoVenda).HasColumnType("decimal(18,2)");
            e.Property(p => p.PrecoCusto).HasColumnType("decimal(18,2)");
            e.HasIndex(p => p.CodigoBarras).IsUnique();
        });

        modelBuilder.Entity<EstoqueFilial>(e =>
        {
            e.HasIndex(x => new { x.FilialId, x.ProdutoId }).IsUnique();
        });

        modelBuilder.Entity<Fornecedor>(e =>
        {
            e.HasIndex(f => f.Cnpj).IsUnique();
        });

        modelBuilder.Entity<Funcionario>(e =>
        {
            e.HasIndex(f => f.Cpf).IsUnique();
        });

        modelBuilder.Entity<Venda>(e =>
        {
            e.Property(v => v.ValorTotal).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ItemVenda>(e =>
        {
            e.Property(i => i.PrecoUnitario).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<OrdemCompra>(e =>
        {
            e.Property(o => o.ValorTotal).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ItemOrdemCompra>(e =>
        {
            e.Property(i => i.PrecoUnitario).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<DespesaOperacional>(e =>
        {
            e.Property(d => d.Valor).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Filial>(e =>
        {
            e.HasIndex(f => f.Cnpj).IsUnique();
        });
    }
}
