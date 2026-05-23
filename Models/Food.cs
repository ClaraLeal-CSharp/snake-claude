namespace SnakeClaude.Models;

/// <summary>
/// Representa um item de comida no grid.
/// </summary>
/// <param name="Position">Posição da comida no grid.</param>
/// <param name="Points">Pontos base que a comida vale.</param>
/// <param name="SpawnedAt">Momento em que a comida foi gerada.</param>
public record Food(Position Position, int Points, DateTime SpawnedAt)
{
    /// <summary>
    /// Cria uma comida padrão na posição fornecida.
    /// </summary>
    public static Food Create(Position position, int basePoints = 10)
        => new(position, basePoints, DateTime.UtcNow);
}
