namespace SnakeClaude.Models;

/// <summary>
/// Representa uma posição no grid 2D do jogo.
/// Imutável (record) para segurança e comparação por valor.
/// </summary>
/// <param name="X">Coluna (eixo horizontal).</param>
/// <param name="Y">Linha (eixo vertical).</param>
public readonly record struct Position(int X, int Y)
{
    /// <summary>
    /// Retorna uma nova posição deslocada pelos deltas fornecidos.
    /// </summary>
    public Position Translate(int dx, int dy) => new(X + dx, Y + dy);

    /// <summary>
    /// Verifica se a posição está dentro dos limites do grid.
    /// </summary>
    public bool IsWithinBounds(int width, int height)
        => X >= 0 && X < width && Y >= 0 && Y < height;

    public override string ToString() => $"({X}, {Y})";
}
