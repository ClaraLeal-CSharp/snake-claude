using SnakeClaude.Enums;

namespace SnakeClaude.Models.Shop;

/// <summary>
/// Classe base abstrata para todos os itens da loja.
/// Todo item concreto deve herdar desta classe.
/// </summary>
public abstract class ShopItemBase
{
    public string Id          { get; init; } = string.Empty;
    public string Name        { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ShopCategory Category { get; init; }
    public ItemRarity Rarity  { get; init; } = ItemRarity.Common;
    public int Price          { get; init; }
    public string Icon        { get; init; } = "▪";
    public string? PreviewTag { get; init; }
    public bool IsAvailable   { get; init; } = true;
    public string MinVersion  { get; init; } = "1.0.0";
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>Item cosmético da loja.</summary>
public class CosmeticItem : ShopItemBase
{
    public CosmeticType CosmeticType { get; init; }
    public string? PreviewCss { get; init; }
}

/// <summary>Item consumível/boost da loja.</summary>
public class ConsumableItem : ShopItemBase
{
    public ItemType ItemType            { get; init; }
    public int DurationSeconds          { get; init; }
    public int QuantityPerPurchase      { get; init; } = 1;
}

/// <summary>Modo de jogo desbloqueável da loja.</summary>
public class GameModeItem : ShopItemBase
{
    public GameModeType GameModeType { get; init; }
    public Dictionary<string, object> ModeModifiers { get; init; } = new();
}
