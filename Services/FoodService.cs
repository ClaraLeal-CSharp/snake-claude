using SnakeClaude.Interfaces;
using SnakeClaude.Models;

namespace SnakeClaude.Services;

/// <summary>
/// Serviço responsável pelo spawn e gerenciamento de comida.
/// Garante que a comida nunca apareça dentro da cobra ou em posição ocupada.
/// </summary>
public class FoodService(ILogger<FoodService> logger) : IFoodService
{
    private readonly Random _random = Random.Shared;

    /// <inheritdoc/>
    public Food SpawnFood(GameGrid grid, IEnumerable<Position> occupiedPositions)
    {
        var freePositions = grid.GetFreePositions(occupiedPositions).ToList();

        if (freePositions.Count == 0)
        {
            logger.LogWarning("Nenhuma posição livre para spawn de comida. Grid lotado?");
            // Retorna posição inválida — a engine deve verificar vitória antes do spawn
            return Food.Create(new Position(-1, -1), 0);
        }

        var position = freePositions[_random.Next(freePositions.Count)];
        var food = Food.Create(position);

        logger.LogDebug("Comida gerada em {Position}", position);
        return food;
    }

    /// <inheritdoc/>
    public bool IsFood(Position position, IEnumerable<Food> activeFoods)
        => activeFoods.Any(f => f.Position == position);

    /// <inheritdoc/>
    public int Collect(Food food)
    {
        logger.LogDebug("Comida coletada em {Position} — {Points} pontos base", food.Position, food.Points);
        return food.Points;
    }
}
