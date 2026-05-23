namespace SnakeClaude.Enums;

/// <summary>
/// Modo de colisão com as bordas do mapa.
/// Preparado para modos diferentes sem alterar a engine.
/// </summary>
public enum WallMode
{
    /// <summary>Colisão com paredes mata o jogador.</summary>
    Solid,

    /// <summary>A cobra atravessa a parede e aparece do outro lado.</summary>
    WrapAround
}
