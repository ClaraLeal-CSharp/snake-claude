using SnakeClaude.Enums;
using SnakeClaude.Models;

namespace SnakeClaude.State;

/// <summary>
/// Snapshot completo e imutável do estado do jogo em um instante.
/// 
/// Decisão arquitetural:
/// - Imutável (record) → thread-safe, seguro para passar ao front-end
/// - Serializable → preparado para PWA/offline (localStorage, IndexedDB)
/// - Separado da engine → a engine tem estado mutável, o front-end recebe snapshots
/// - Preparado para replay e persistência de high score
/// </summary>
public record GameStateSnapshot
{
    /// <summary>Status atual do jogo.</summary>
    public GameStatus Status { get; init; }

    /// <summary>Segmentos da cobra (índice 0 = cabeça).</summary>
    public IReadOnlyList<Position> SnakeBody { get; init; } = [];

    /// <summary>Direção atual de movimento.</summary>
    public Direction CurrentDirection { get; init; }

    /// <summary>Posições de todas as comidas no mapa.</summary>
    public IReadOnlyList<Position> FoodPositions { get; init; } = [];

    /// <summary>Placar atual.</summary>
    public ScoreBoardSnapshot Score { get; init; } = null!;

    /// <summary>Largura do grid.</summary>
    public int GridWidth { get; init; }

    /// <summary>Altura do grid.</summary>
    public int GridHeight { get; init; }

    /// <summary>Tick atual (frame do jogo).</summary>
    public long TickCount { get; init; }

    /// <summary>Timestamp do snapshot (UTC).</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Motivo do game over (null se jogo em curso).</summary>
    public string? GameOverReason { get; init; }

    /// <summary>Modo de parede ativo.</summary>
    public WallMode WallMode { get; init; }

    /// <summary>Intervalo de tick atual em ms (reflete velocidade atual).</summary>
    public int CurrentTickIntervalMs { get; init; }

    /// <summary>Dificuldade aplicada na partida atual.</summary>
    public DifficultyLevel Difficulty { get; init; } = DifficultyLevel.Normal;
}
