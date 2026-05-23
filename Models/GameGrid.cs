namespace SnakeClaude.Models;

/// <summary>
/// Representa o grid do jogo — a "tela" lógica onde tudo acontece.
/// Independente de qualquer UI. A renderização é responsabilidade do front-end.
/// </summary>
public class GameGrid
{
    public int Width { get; }
    public int Height { get; }
    public int TotalCells => Width * Height;

    public GameGrid(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Aplica WrapAround: a posição "sai" por uma borda e aparece na oposta.
    /// </summary>
    public Position Wrap(Position position) =>
        new(
            ((position.X % Width) + Width) % Width,
            ((position.Y % Height) + Height) % Height
        );

    /// <summary>
    /// Verifica se uma posição está dentro dos limites do grid.
    /// </summary>
    public bool IsInBounds(Position position) =>
        position.IsWithinBounds(Width, Height);

    /// <summary>
    /// Retorna todas as posições livres (não ocupadas pela cobra nem por comida).
    /// Usado para spawn de comida.
    /// </summary>
    public IEnumerable<Position> GetFreePositions(IEnumerable<Position> occupied)
    {
        var occupiedSet = new HashSet<Position>(occupied);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                var pos = new Position(x, y);
                if (!occupiedSet.Contains(pos))
                    yield return pos;
            }
    }
}
