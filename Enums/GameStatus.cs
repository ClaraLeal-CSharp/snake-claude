namespace SnakeClaude.Enums;

/// <summary>
/// Estado atual do jogo.
/// </summary>
public enum GameStatus
{
    /// <summary>Jogo ainda não iniciado.</summary>
    Idle,

    /// <summary>Jogo em andamento.</summary>
    Running,

    /// <summary>Jogo pausado pelo jogador.</summary>
    Paused,

    /// <summary>Jogo encerrado por derrota.</summary>
    GameOver,

    /// <summary>Jogo encerrado por vitória (mapa preenchido).</summary>
    Victory
}
