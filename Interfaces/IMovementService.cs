using SnakeClaude.Enums;
using SnakeClaude.Models;

namespace SnakeClaude.Interfaces;

/// <summary>
/// Contrato para o serviço de movimentação da cobra.
/// Centraliza cálculo de próxima posição e detecção de colisão.
/// </summary>
public interface IMovementService
{
    /// <summary>
    /// Calcula a próxima posição da cabeça baseado na direção e modo de parede.
    /// </summary>
    Position CalculateNextHead(Position currentHead, Direction direction, GameGrid grid, WallMode wallMode);

    /// <summary>
    /// Verifica se a próxima posição causa colisão com parede (no modo Solid).
    /// </summary>
    bool IsWallCollision(Position nextHead, GameGrid grid, WallMode wallMode);

    /// <summary>
    /// Verifica se a próxima posição causa auto-colisão com o corpo da cobra.
    /// </summary>
    bool IsSelfCollision(Position nextHead, Snake snake);

    /// <summary>
    /// Retorna o delta (dx, dy) para uma dada direção.
    /// </summary>
    (int dx, int dy) GetDirectionDelta(Direction direction);
}
