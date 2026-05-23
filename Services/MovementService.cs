using SnakeClaude.Enums;
using SnakeClaude.Interfaces;
using SnakeClaude.Models;

namespace SnakeClaude.Services;

/// <summary>
/// Serviço de movimentação da cobra — compatível com Blazor WASM (sem ILogger).
/// </summary>
public class MovementService : IMovementService
{
    public (int dx, int dy) GetDirectionDelta(Direction direction) => direction switch
    {
        Direction.Up    => (0, -1),
        Direction.Down  => (0,  1),
        Direction.Left  => (-1, 0),
        Direction.Right => ( 1, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };

    public Position CalculateNextHead(Position currentHead, Direction direction, GameGrid grid, WallMode wallMode)
    {
        var (dx, dy) = GetDirectionDelta(direction);
        var next = currentHead.Translate(dx, dy);

        if (wallMode == WallMode.WrapAround)
            next = grid.Wrap(next);

        return next;
    }

    public bool IsWallCollision(Position nextHead, GameGrid grid, WallMode wallMode)
    {
        if (wallMode == WallMode.WrapAround) return false;
        return !grid.IsInBounds(nextHead);
    }

    public bool IsSelfCollision(Position nextHead, Snake snake)
    {
        var bodyWithoutTail = snake.Body.Take(snake.Body.Count - 1);
        return bodyWithoutTail.Contains(nextHead);
    }
}
