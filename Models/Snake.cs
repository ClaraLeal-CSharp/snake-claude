using SnakeClaude.Enums;

namespace SnakeClaude.Models;

/// <summary>
/// Representa a cobra no jogo.
/// O corpo é uma fila duplamente encadeada para crescimento/movimentação eficiente O(1).
/// </summary>
public class Snake
{
    // LinkedList permite inserção na frente (nova cabeça) e remoção no final (cauda) em O(1)
    private readonly LinkedList<Position> _body = new();

    /// <summary>Segmentos do corpo da cobra (cabeça → cauda).</summary>
    public IReadOnlyCollection<Position> Body => _body;

    /// <summary>Posição atual da cabeça.</summary>
    public Position Head => _body.First!.Value;

    /// <summary>Direção atual de movimento.</summary>
    public Direction CurrentDirection { get; private set; }

    /// <summary>
    /// Direção que será aplicada no próximo tick.
    /// Protege contra reversão instantânea e múltiplos inputs no mesmo tick.
    /// </summary>
    public Direction NextDirection { get; private set; }

    /// <summary>Comprimento atual da cobra.</summary>
    public int Length => _body.Count;

    /// <summary>
    /// Inicializa a cobra com posição e direção iniciais.
    /// </summary>
    public Snake(Position startPosition, Direction startDirection)
    {
        _body.AddFirst(startPosition);
        CurrentDirection = startDirection;
        NextDirection = startDirection;
    }

    /// <summary>
    /// Solicita mudança de direção. Impede reversão instantânea (ex: cima → baixo).
    /// A mudança só será aplicada no próximo tick para evitar cheating com inputs rápidos.
    /// </summary>
    public void RequestDirectionChange(Direction newDirection)
    {
        if (IsOppositeDirection(CurrentDirection, newDirection))
            return;

        NextDirection = newDirection;
    }

    /// <summary>
    /// Aplica o movimento: adiciona nova cabeça, retorna se cresceu ou moveu.
    /// Se <paramref name="grow"/> for false, remove a cauda (movimento normal).
    /// </summary>
    /// <returns>A posição da cauda removida, ou null se cresceu.</returns>
    public Position? Move(Position newHead, bool grow = false)
    {
        CurrentDirection = NextDirection;
        _body.AddFirst(newHead);

        if (grow)
            return null;

        var tail = _body.Last!.Value;
        _body.RemoveLast();
        return tail;
    }

    /// <summary>
    /// Verifica se a cobra ocupa determinada posição.
    /// </summary>
    public bool OccupiesPosition(Position position)
        => _body.Contains(position);

    /// <summary>
    /// Verifica se a cabeça colidiu com o próprio corpo (ignora a cabeça atual).
    /// </summary>
    public bool IsSelfColliding()
    {
        var node = _body.First?.Next; // Começa pelo segundo elemento
        while (node is not null)
        {
            if (node.Value == Head)
                return true;
            node = node.Next;
        }
        return false;
    }

    /// <summary>
    /// Retorna os segmentos do corpo como lista para serialização/snapshot.
    /// </summary>
    public List<Position> GetBodySnapshot() => [.. _body];

    private static bool IsOppositeDirection(Direction current, Direction requested) =>
        (current == Direction.Up && requested == Direction.Down) ||
        (current == Direction.Down && requested == Direction.Up) ||
        (current == Direction.Left && requested == Direction.Right) ||
        (current == Direction.Right && requested == Direction.Left);
}
