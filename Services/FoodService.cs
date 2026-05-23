using SnakeClaude.Interfaces;
using SnakeClaude.Models;

namespace SnakeClaude.Services;

/// <summary>
/// Serviço de spawn de comida — compatível com Blazor WASM (sem ILogger).
/// </summary>
public class FoodService : IFoodService
{
    private readonly Random _random = Random.Shared;

    public Food SpawnFood(GameGrid grid, IEnumerable<Position> occupiedPositions)
    {
        var freePositions = grid.GetFreePositions(occupiedPositions).ToList();

        if (freePositions.Count == 0)
            return Food.Create(new Position(-1, -1), 0);

        var position = freePositions[_random.Next(freePositions.Count)];
        return Food.Create(position);
    }

    public bool IsFood(Position position, IEnumerable<Food> activeFoods)
        => activeFoods.Any(f => f.Position == position);

    public int Collect(Food food)
        => food.Points;
}
