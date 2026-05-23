using SnakeClaude.Enums;
using SnakeClaude.Models;
using SnakeClaude.State;

namespace SnakeClaude.DTOs;

/// <summary>
/// DTO do estado do jogo enviado ao front-end em cada tick.
/// Contém apenas o necessário para renderização — sem lógica interna.
/// Preparado para serialização JSON (SignalR, REST, SSE).
/// </summary>
public class GameStateDto
{
    public string Status { get; set; } = string.Empty;
    public List<PositionDto> SnakeBody { get; set; } = [];
    public List<PositionDto> FoodPositions { get; set; } = [];
    public string Direction { get; set; } = string.Empty;
    public ScoreDto Score { get; set; } = new();
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public long TickCount { get; set; }
    public string? GameOverReason { get; set; }
    public string WallMode { get; set; } = string.Empty;
    public int CurrentTickIntervalMs { get; set; }

    /// <summary>Cria DTO a partir do snapshot interno.</summary>
    public static GameStateDto FromSnapshot(GameStateSnapshot snapshot) => new()
    {
        Status = snapshot.Status.ToString(),
        SnakeBody = snapshot.SnakeBody.Select(p => new PositionDto(p.X, p.Y)).ToList(),
        FoodPositions = snapshot.FoodPositions.Select(p => new PositionDto(p.X, p.Y)).ToList(),
        Direction = snapshot.CurrentDirection.ToString(),
        Score = ScoreDto.FromSnapshot(snapshot.Score),
        GridWidth = snapshot.GridWidth,
        GridHeight = snapshot.GridHeight,
        TickCount = snapshot.TickCount,
        GameOverReason = snapshot.GameOverReason,
        WallMode = snapshot.WallMode.ToString(),
        CurrentTickIntervalMs = snapshot.CurrentTickIntervalMs
    };
}

/// <summary>DTO de posição 2D.</summary>
public record PositionDto(int X, int Y);

/// <summary>DTO do placar.</summary>
public class ScoreDto
{
    public int TotalScore { get; set; }
    public int ComboMultiplier { get; set; }
    public int ComboCount { get; set; }
    public int MaxCombo { get; set; }
    public int HighestSingleScore { get; set; }

    public static ScoreDto FromSnapshot(Models.ScoreBoardSnapshot s) => new()
    {
        TotalScore = s.TotalScore,
        ComboMultiplier = s.ComboMultiplier,
        ComboCount = s.ComboCount,
        MaxCombo = s.MaxCombo,
        HighestSingleScore = s.HighestSingleScore
    };
}

/// <summary>
/// DTO de comando de direção enviado pelo front-end.
/// </summary>
public class DirectionCommandDto
{
    /// <summary>Direção: "Up", "Down", "Left", "Right".</summary>
    public string Direction { get; set; } = string.Empty;

    public Enums.Direction ToEnum() =>
        Enum.TryParse<Enums.Direction>(Direction, ignoreCase: true, out var dir)
            ? dir
            : throw new ArgumentException($"Direção inválida: {Direction}");
}
