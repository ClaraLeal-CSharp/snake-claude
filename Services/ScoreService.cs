using Microsoft.Extensions.Options;
using SnakeClaude.Configuration;
using SnakeClaude.Events;
using SnakeClaude.Interfaces;
using SnakeClaude.Models;

namespace SnakeClaude.Services;

/// <summary>
/// Serviço de pontuação com sistema de combo.
/// 
/// Sistema de combo:
/// - Cada comida coletada dentro da janela de tempo aumenta o multiplicador.
/// - Se o tempo entre coletas exceder ComboWindowMs, o combo reinicia.
/// - O multiplicador máximo é limitado por MaxComboMultiplier.
/// - Eventos são disparados para o front-end reagir (animações, sons etc).
/// </summary>
public class ScoreService(
    IOptions<GameSettings> settings,
    ILogger<ScoreService> logger) : IScoreService
{
    private readonly GameSettings _settings = settings.Value;

    public ScoreBoard ScoreBoard { get; private set; } = new();

    // Eventos para o front-end (ex: animação de combo)
    public event EventHandler<ComboChangedEventArgs>? OnComboChanged;
    public event EventHandler<ComboResetEventArgs>? OnComboReset;

    /// <inheritdoc/>
    public int ProcessFoodCollection(int basePoints)
    {
        // Verifica se ainda está na janela de combo antes de incrementar
        CheckComboExpiry();

        ScoreBoard.IncrementCombo(_settings.MaxComboMultiplier);
        var earned = ScoreBoard.AddScore(basePoints);

        logger.LogDebug(
            "Comida coletada: +{Points} pts (x{Multiplier} combo, combo #{Count})",
            earned, ScoreBoard.ComboMultiplier, ScoreBoard.ComboCount);

        OnComboChanged?.Invoke(this, new ComboChangedEventArgs(
            ScoreBoard.ComboMultiplier,
            ScoreBoard.ComboCount));

        return earned;
    }

    /// <inheritdoc/>
    public void CheckComboExpiry()
    {
        if (ScoreBoard.LastComboTime == DateTime.MinValue)
            return; // Ainda não teve combo — nada a verificar

        var elapsed = (DateTime.UtcNow - ScoreBoard.LastComboTime).TotalMilliseconds;
        if (elapsed > _settings.ComboWindowMs && ScoreBoard.ComboCount > 0)
        {
            var finalCombo = ScoreBoard.ComboMultiplier;
            ScoreBoard.ResetCombo();

            logger.LogDebug("Combo expirado após {Elapsed}ms — multiplicador resetado", elapsed);
            OnComboReset?.Invoke(this, new ComboResetEventArgs(finalCombo));
        }
    }

    /// <inheritdoc/>
    public void Reset()
    {
        ScoreBoard = new ScoreBoard();
        logger.LogDebug("ScoreService resetado");
    }
}
