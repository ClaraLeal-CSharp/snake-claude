using SnakeClaude.Enums;
using SnakeClaude.Interfaces;
using SnakeClaude.Models;

namespace SnakeClaude.Services;

/// <summary>
/// Serviço de movimentação da cobra.
/// Centraliza toda a lógica de direção, colisão e wrap-around.
/// </summary>
public class MovementService(ILogger<MovementService> logger) : IMovementService
{
    /// <inheritdoc/>
    public (int dx, int dy) GetDirectionDelta(Direction direction) => direction switch
    {
        Direction.Up    => (0, -1),
        Direction.Down  => (0,  1),
        Direction.Left  => (-1, 0),
        Direction.Right => ( 1, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };

    /// <inheritdoc/>
    public Position CalculateNextHead(Position currentHead, Direction direction, GameGrid grid, WallMode wallMode)
    {
        var (dx, dy) = GetDirectionDelta(direction);
        var next = currentHead.Translate(dx, dy);

        // WrapAround: atravessa a borda e aparece do outro lado
        if (wallMode == WallMode.WrapAround)
            next = grid.Wrap(next);

        return next;
    }

    /// <inheritdoc/>
    public bool IsWallCollision(Position nextHead, GameGrid grid, WallMode wallMode)
    {
        if (wallMode == WallMode.WrapAround)
            return false; // Nunca colide com parede no modo wrap

        var collision = !grid.IsInBounds(nextHead);
        if (collision)
            logger.LogDebug("Colisão com parede em {Position}", nextHead);

        return collision;
    }

    /// <inheritdoc/>
    public bool IsSelfCollision(Position nextHead, Snake snake)
    {
        // Verifica se a nova cabeça colide com qualquer segmento do corpo atual
        // exceto a cauda, que será removida no mesmo tick
        // Logo, comparamos nextHead com todos os segmentos exceto o último
        var bodyWithoutTail = snake.Body.Take(snake.Body.Count - 1);
        var collision = bodyWithoutTail.Contains(nextHead);

        if (collision)
            logger.LogDebug("Auto-colisão em {Position}", nextHead);

        return collision;
    }
}
