using SnakeClaude.Enums;
using SnakeClaude.Events;
using SnakeClaude.State;

namespace SnakeClaude.Interfaces;

/// <summary>
/// Contrato principal da engine do jogo.
/// O front-end (e qualquer outro consumidor) só interage por esta interface.
/// Isso desacopla completamente a UI da implementação da engine.
/// </summary>
public interface IGameEngine : IAsyncDisposable
{
    // ── Estado ─────────────────────────────────────────────────────────────
    GameStatus Status { get; }
    GameStateSnapshot CurrentSnapshot { get; }

    // ── Eventos (Observer Pattern) ─────────────────────────────────────────
    event EventHandler<GameTickEventArgs> OnTick;
    event EventHandler<FoodCollectedEventArgs> OnFoodCollected;
    event EventHandler<GameStatusChangedEventArgs> OnStatusChanged;
    event EventHandler<GameOverEventArgs> OnGameOver;
    event EventHandler<FoodSpawnedEventArgs> OnFoodSpawned;
    event EventHandler<ComboChangedEventArgs> OnComboChanged;
    event EventHandler<ComboResetEventArgs> OnComboReset;

    // ── Controle ───────────────────────────────────────────────────────────
    Task StartAsync();
    Task PauseAsync();
    Task ResumeAsync();
    Task ResetAsync();

    /// <summary>Enfileira uma mudança de direção para o próximo tick.</summary>
    void RequestDirectionChange(Direction direction);
}
