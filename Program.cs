using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SnakeClaude.Components;
using SnakeClaude.Configuration;
using SnakeClaude.Engine;
using SnakeClaude.Interfaces;
using SnakeClaude.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── Configuração do jogo ───────────────────────────────────────────────────
builder.Services.AddSingleton(new GameSettings
{
    GridWidth          = 20,
    GridHeight         = 20,
    TickIntervalMs     = 150,
    MinTickIntervalMs  = 50,
    SpeedIncrementMs   = 5,
    InitialSnakeLength = 3,
    WallMode           = SnakeClaude.Enums.WallMode.Solid,
    BasePointsPerFood  = 10,
    ComboWindowMs      = 3000,
    MaxComboMultiplier = 5,
    MaxFoodItems       = 1,
    MaxPlayers         = 1
});

// ── Serviços do Jogo ───────────────────────────────────────────────────────
builder.Services.AddSingleton<IMovementService, MovementService>();
builder.Services.AddSingleton<IFoodService, FoodService>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<IGameEngine, GameEngine>();

await builder.Build().RunAsync();
