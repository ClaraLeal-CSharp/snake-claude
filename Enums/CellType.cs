namespace SnakeClaude.Enums;

/// <summary>
/// Tipo de célula no grid do jogo.
/// Usado para renderização e lógica de colisão.
/// </summary>
public enum CellType
{
    Empty,
    SnakeHead,
    SnakeBody,
    Food,
    Wall
}
