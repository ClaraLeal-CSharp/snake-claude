using SnakeClaude.Enums;
using SnakeClaude.Models;
using SnakeClaude.State;

namespace SnakeClaude.Events;


// ============================================================
// Argumentos de eventos do jogo Snake
// Cada evento tem seu próprio EventArgs tipado para extensibilidade.
// O front-end (Blazor) e outros serviços subscrevem esses eventos
// sem nenhum acoplamento com a engine.
// ============================================================

/// <summary>
/// Evento disparado a cada tick do game loop.
/// Carrega o estado completo para re-renderização.
/// </summary>
public class GameTickEventArgs(GameStateSnapshot snapshot) : EventArgs
{
    public GameStateSnapshot Snapshot { get; } = snapshot;
}

/// <summary>
/// Evento disparado quando uma comida é coletada.
/// </summary>
public class FoodCollectedEventArgs(
    Position foodPosition,
    int pointsEarned,
    int comboMultiplier,
    int newComboCount) : EventArgs
{
    public Position FoodPosition { get; } = foodPosition;
    public int PointsEarned { get; } = pointsEarned;
    public int ComboMultiplier { get; } = comboMultiplier;
    public int NewComboCount { get; } = newComboCount;
}

/// <summary>
/// Evento disparado quando o combo é incrementado.
/// </summary>
public class ComboChangedEventArgs(int comboMultiplier, int comboCount) : EventArgs
{
    public int ComboMultiplier { get; } = comboMultiplier;
    public int ComboCount { get; } = comboCount;
}

/// <summary>
/// Evento disparado quando o combo é resetado (timeout ou derrota).
/// </summary>
public class ComboResetEventArgs(int finalCombo) : EventArgs
{
    public int FinalCombo { get; } = finalCombo;
}

/// <summary>
/// Evento disparado quando o status do jogo muda.
/// </summary>
public class GameStatusChangedEventArgs(GameStatus previousStatus, GameStatus newStatus) : EventArgs
{
    public GameStatus PreviousStatus { get; } = previousStatus;
    public GameStatus NewStatus { get; } = newStatus;
}

/// <summary>
/// Evento disparado quando o jogo termina (game over ou vitória).
/// </summary>
public class GameOverEventArgs(GameStateSnapshot finalSnapshot, string reason) : EventArgs
{
    public GameStateSnapshot FinalSnapshot { get; } = finalSnapshot;
    public string Reason { get; } = reason;
}

/// <summary>
/// Evento disparado quando uma nova comida é gerada no mapa.
/// </summary>
public class FoodSpawnedEventArgs(Position position, int points) : EventArgs
{
    public Position Position { get; } = position;
    public int Points { get; } = points;
}
