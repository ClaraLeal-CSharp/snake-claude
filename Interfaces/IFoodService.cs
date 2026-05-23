using SnakeClaude.Models;

namespace SnakeClaude.Interfaces;

/// <summary>
/// Contrato para o serviço de spawn e gerenciamento de comida.
/// </summary>
public interface IFoodService
{
    /// <summary>
    /// Gera um item de comida em posição aleatória não ocupada pela cobra.
    /// </summary>
    Food SpawnFood(GameGrid grid, IEnumerable<Position> occupiedPositions);

    /// <summary>
    /// Verifica se uma posição contém comida.
    /// </summary>
    bool IsFood(Position position, IEnumerable<Food> activeFoods);

    /// <summary>
    /// Remove a comida coletada e retorna os pontos base dela.
    /// </summary>
    int Collect(Food food);
}
