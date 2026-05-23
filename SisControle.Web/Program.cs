using Microsoft.EntityFrameworkCore;
using SisControle.Application.Services;
using SisControle.Domain.Interfaces;
using SisControle.Infrastructure.Data;
using SisControle.Infrastructure.Repositories;
using SisControle.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// EF Core com SQL Server via TCP (Regra de Infraestrutura)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositórios
builder.Services.AddScoped<IFilialRepository, FilialRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IFornecedorRepository, FornecedorRepository>();
builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IVendaRepository, VendaRepository>();
builder.Services.AddScoped<IEstoqueRepository, EstoqueRepository>();
builder.Services.AddScoped<IDespesaRepository, DespesaRepository>();
builder.Services.AddScoped<IOrdemCompraRepository, OrdemCompraRepository>();

// Services (regras de negócio)
builder.Services.AddScoped<FilialService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<FornecedorService>();
builder.Services.AddScoped<FuncionarioService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<VendaService>();
builder.Services.AddScoped<DespesaService>();
builder.Services.AddScoped<OrdemCompraService>();
builder.Services.AddScoped<RelatorioService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Auto-migração e seed na inicialização
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
