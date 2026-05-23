using SnakeClaude.Enums;

namespace SnakeClaude.Configuration;

/// <summary>
/// Configuração central do jogo Snake.
/// Injetada via IOptions&lt;GameSettings&gt; — configurável por appsettings.json ou código.
/// Toda decisão de gameplay está aqui para evitar "magic numbers" espalhados.
/// </summary>
public class GameSettings
{
    public const string SectionName = "GameSettings";

    // ── Grid ───────────────────────────────────────────────────────────────
    /// <summary>Largura do grid (colunas).</summary>
    public int GridWidth { get; set; } = 20;

    /// <summary>Altura do grid (linhas).</summary>
    public int GridHeight { get; set; } = 20;

    // ── Game Loop ──────────────────────────────────────────────────────────
    /// <summary>Intervalo base entre ticks em milissegundos.</summary>
    public int TickIntervalMs { get; set; } = 150;

    /// <summary>Velocidade mínima (intervalo mínimo entre ticks em ms).</summary>
    public int MinTickIntervalMs { get; set; } = 50;

    /// <summary>
    /// Redução de ms por comida coletada (acelera o jogo progressivamente).
    /// </summary>
    public int SpeedIncrementMs { get; set; } = 5;

    // ── Cobra ──────────────────────────────────────────────────────────────
    /// <summary>Tamanho inicial da cobra (número de segmentos).</summary>
    public int InitialSnakeLength { get; set; } = 3;

    // ── Paredes ────────────────────────────────────────────────────────────
    /// <summary>Comportamento das bordas do mapa.</summary>
    public WallMode WallMode { get; set; } = WallMode.Solid;

    // ── Pontuação ──────────────────────────────────────────────────────────
    /// <summary>Pontos base por comida coletada.</summary>
    public int BasePointsPerFood { get; set; } = 10;

    // ── Combo ──────────────────────────────────────────────────────────────
    /// <summary>Janela de tempo (em ms) para manter o combo ativo.</summary>
    public int ComboWindowMs { get; set; } = 3000;

    /// <summary>Multiplicador máximo atingível pelo combo.</summary>
    public int MaxComboMultiplier { get; set; } = 5;

    // ── Comida ─────────────────────────────────────────────────────────────
    /// <summary>Número de itens de comida simultâneos no mapa.</summary>
    public int MaxFoodItems { get; set; } = 1;

    // ── Multiplayer (preparado para futuro) ────────────────────────────────
    /// <summary>Número máximo de jogadores (1 = single player).</summary>
    public int MaxPlayers { get; set; } = 1;
}
