namespace SnakeClaude.Models;

/// <summary>
/// Representa o sistema de pontuação e combo em um dado momento.
/// </summary>
public class ScoreBoard
{
    /// <summary>Pontuação total acumulada.</summary>
    public int TotalScore { get; private set; }

    /// <summary>Multiplicador de combo atual (mínimo 1).</summary>
    public int ComboMultiplier { get; private set; } = 1;

    /// <summary>Quantidade de comidas pegas no combo atual.</summary>
    public int ComboCount { get; private set; }

    /// <summary>Maior combo alcançado na partida.</summary>
    public int MaxCombo { get; private set; }

    /// <summary>Maior pontuação única registrada.</summary>
    public int HighestSingleScore { get; private set; }

    /// <summary>Momento do último ponto de combo (para timeout).</summary>
    public DateTime LastComboTime { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Adiciona pontos com o multiplicador de combo atual.
    /// </summary>
    /// <returns>Os pontos realmente adicionados (base × multiplicador).</returns>
    public int AddScore(int basePoints)
    {
        var earned = basePoints * ComboMultiplier;
        TotalScore += earned;

        if (earned > HighestSingleScore)
            HighestSingleScore = earned;

        return earned;
    }

    /// <summary>
    /// Incrementa o combo. Deve ser chamado pelo ComboService após validação de tempo.
    /// </summary>
    public void IncrementCombo(int maxMultiplier)
    {
        ComboCount++;
        ComboMultiplier = Math.Min(ComboCount, maxMultiplier);
        LastComboTime = DateTime.UtcNow;

        if (ComboMultiplier > MaxCombo)
            MaxCombo = ComboMultiplier;
    }

    /// <summary>
    /// Reinicia o combo (tempo expirado ou colisão).
    /// </summary>
    public void ResetCombo()
    {
        ComboCount = 0;
        ComboMultiplier = 1;
    }

    /// <summary>
    /// Snapshot imutável para serialização e exibição no front-end.
    /// </summary>
    public ScoreBoardSnapshot ToSnapshot() => new(
        TotalScore,
        ComboMultiplier,
        ComboCount,
        MaxCombo,
        HighestSingleScore,
        LastComboTime
    );
}

/// <summary>
/// Snapshot imutável do placar — seguro para passar ao front-end ou serializar.
/// </summary>
public record ScoreBoardSnapshot(
    int TotalScore,
    int ComboMultiplier,
    int ComboCount,
    int MaxCombo,
    int HighestSingleScore,
    DateTime LastComboTime
);
