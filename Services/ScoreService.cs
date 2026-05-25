using SnakeClaude.Configuration;
using SnakeClaude.Events;
using SnakeClaude.Interfaces;
using SnakeClaude.Models;

namespace SnakeClaude.Services;

/// <summary>
/// Serviço de pontuação com sistema de combo — compatível com Blazor WASM.
/// Recebe GameSettings diretamente (sem IOptions) e não usa ILogger.
/// </summary>
public class ScoreService(GameSettings settings) : IScoreService
{
    private readonly GameSettings _settings = settings;

    public ScoreBoard ScoreBoard { get; private set; } = new();

    public event EventHandler<ComboChangedEventArgs>? OnComboChanged;
    public event EventHandler<ComboResetEventArgs>? OnComboReset;

    public int ProcessFoodCollection(int basePoints)
    {
        CheckComboExpiry();
        var maxCombo = Math.Max(1, (int)Math.Round(_settings.MaxComboMultiplier * _settings.ComboMultiplier));
        var scaledPoints = Math.Max(1, (int)Math.Round(basePoints * _settings.PointMultiplier));

        ScoreBoard.IncrementCombo(maxCombo);
        var earned = ScoreBoard.AddScore(scaledPoints);
        OnComboChanged?.Invoke(this, new ComboChangedEventArgs(
            ScoreBoard.ComboMultiplier,
            ScoreBoard.ComboCount));
        return earned;
    }

    public void CheckComboExpiry()
    {
        if (ScoreBoard.LastComboTime == DateTime.MinValue) return;

        var elapsed = (DateTime.UtcNow - ScoreBoard.LastComboTime).TotalMilliseconds;
        if (elapsed > _settings.ComboWindowMs && ScoreBoard.ComboCount > 0)
        {
            var finalCombo = ScoreBoard.ComboMultiplier;
            ScoreBoard.ResetCombo();
            OnComboReset?.Invoke(this, new ComboResetEventArgs(finalCombo));
        }
    }

    public void Reset()
    {
        ScoreBoard = new ScoreBoard();
    }
}
