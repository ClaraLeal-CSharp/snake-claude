using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SnakeClaude.Components;
using SnakeClaude.Configuration;
using SnakeClaude.Engine;
using SnakeClaude.Interfaces;
using SnakeClaude.Interfaces.Shop;
using SnakeClaude.Services;
using SnakeClaude.Services.Shop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// â”€â”€ ConfiguraÃ§Ã£o do jogo â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddSingleton(new GameSettings
{
    GridWidth          = 20,
    GridHeight         = 20,
    TickIntervalMs     = 150,
    MinTickIntervalMs  = 50,
    AccelerationIntervalMs = 7000,
    SpeedIncrementMs   = 5,
    InitialSnakeLength = 3,
    WallMode           = SnakeClaude.Enums.WallMode.Solid,
    BasePointsPerFood  = 10,
    PointMultiplier    = 1,
    ComboWindowMs      = 3000,
    MaxComboMultiplier = 5,
    ComboMultiplier    = 1,
    MaxFoodItems       = 2,
    MaxPlayers         = 1
});

// â”€â”€ ServiÃ§os do Jogo â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddSingleton<IMovementService, MovementService>();
builder.Services.AddSingleton<IFoodService, FoodService>();
builder.Services.AddSingleton<IDifficultySettingsService, DifficultySettingsService>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<IGameEngine, GameEngine>();

// -- Servicos da Loja
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IShopService, ShopService>();

await builder.Build().RunAsync();

