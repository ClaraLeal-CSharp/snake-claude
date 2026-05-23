using SnakeClaude.Models;

namespace SnakeClaude.Interfaces;

/// <summary>
/// Contrato para o serviço de pontuação e combo.
/// </summary>
public interface IScoreService
{
    ScoreBoard ScoreBoard { get; }

    /// <summary>
    /// Processa a coleta de uma comida: verifica combo, adiciona pontos.
    /// </summary>
    /// <returns>Pontos efetivamente ganhos (base × multiplicador).</returns>
    int ProcessFoodCollection(int basePoints);

    /// <summary>
    /// Verifica se o combo expirou pelo timeout e reseta se necessário.
    /// Deve ser chamado a cada tick.
    /// </summary>
    void CheckComboExpiry();

    /// <summary>Reseta completamente a pontuação (novo jogo).</summary>
    void Reset();
}
