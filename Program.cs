using SnakeClaude.Components;
using SnakeClaude.Configuration;
using SnakeClaude.Engine;
using SnakeClaude.Interfaces;
using SnakeClaude.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuração ───────────────────────────────────────────────────────────
// GameSettings é lido do appsettings.json e injetado via IOptions<GameSettings>
builder.Services.Configure<GameSettings>(
    builder.Configuration.GetSection(GameSettings.SectionName));

// ── Serviços do Jogo ───────────────────────────────────────────────────────
// Singleton: serviços stateless reutilizados entre todas as sessões
builder.Services.AddSingleton<IMovementService, MovementService>();
builder.Services.AddSingleton<IFoodService, FoodService>();

// Scoped: uma instância de ScoreService e GameEngine por sessão Blazor
// Isso permite múltiplos jogadores independentes no futuro
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<IGameEngine, GameEngine>();

// ── Blazor ─────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────
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
