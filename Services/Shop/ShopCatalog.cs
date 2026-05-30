using SnakeClaude.Enums;
using SnakeClaude.Models.Shop;

namespace SnakeClaude.Services.Shop;

/// <summary>
/// Catálogo central da loja — define todos os itens disponíveis.
/// Para adicionar novos itens, basta incluí-los nas listas abaixo.
/// </summary>
public static class ShopCatalog
{
    public static IReadOnlyList<CosmeticItem> Cosmetics { get; } =
    [
        new CosmeticItem
        {
            Id = "skin_default", Name = "Padrão",
            Description = "A skin clássica do Snake Claude. Sempre disponível.",
            Category = ShopCategory.Cosmetics, CosmeticType = CosmeticType.SnakeSkin,
            Rarity = ItemRarity.Common, Price = 0, Icon = "■", PreviewTag = "DEFAULT", IsAvailable = true
        },
        new CosmeticItem
        {
            Id = "skin_neon", Name = "Neon Pulse",
            Description = "Rastro neon pulsante. Em breve.",
            Category = ShopCategory.Cosmetics, CosmeticType = CosmeticType.SnakeSkin,
            Rarity = ItemRarity.Rare, Price = 50, Icon = "◈", PreviewTag = "EM BREVE", IsAvailable = false
        },
        new CosmeticItem
        {
            Id = "skin_ghost", Name = "Phantasm",
            Description = "Cobra translúcida com aura etérea. Em breve.",
            Category = ShopCategory.Cosmetics, CosmeticType = CosmeticType.SnakeSkin,
            Rarity = ItemRarity.Epic, Price = 150, Icon = "◇", PreviewTag = "EM BREVE", IsAvailable = false
        },
        new CosmeticItem
        {
            Id = "skin_legendary", Name = "Void Serpent",
            Description = "Lendário. Apenas os melhores. Em breve.",
            Category = ShopCategory.Cosmetics, CosmeticType = CosmeticType.SnakeSkin,
            Rarity = ItemRarity.Legendary, Price = 500, Icon = "◆", PreviewTag = "EM BREVE", IsAvailable = false
        }
    ];

    public static IReadOnlyList<ConsumableItem> Items { get; } =
    [
        new ConsumableItem
        {
            Id = "item_continue", Name = "Continue",
            Description = "Continua a partida após game over. Em breve.",
            Category = ShopCategory.Items, ItemType = ItemType.Continue,
            Rarity = ItemRarity.Common, Price = 10, Icon = "↩",
            PreviewTag = "EM BREVE", IsAvailable = false, QuantityPerPurchase = 1
        },
        new ConsumableItem
        {
            Id = "item_score_boost", Name = "Score Boost",
            Description = "Dobra os pontos por 30 segundos. Em breve.",
            Category = ShopCategory.Items, ItemType = ItemType.ScoreBoost,
            Rarity = ItemRarity.Rare, Price = 25, Icon = "×2",
            PreviewTag = "EM BREVE", IsAvailable = false,
            DurationSeconds = 30, QuantityPerPurchase = 1
        },
        new ConsumableItem
        {
            Id = "item_shield", Name = "Shield",
            Description = "Protege de uma colisão. Em breve.",
            Category = ShopCategory.Items, ItemType = ItemType.Shield,
            Rarity = ItemRarity.Rare, Price = 20, Icon = "◉",
            PreviewTag = "EM BREVE", IsAvailable = false, QuantityPerPurchase = 1
        }
    ];

    public static IReadOnlyList<CosmeticItem> Themes { get; } =
    [
        new CosmeticItem
        {
            Id = "theme_snake_aero",
            Name = "Snake Aero",
            Description = "Reviva o futuro otimista dos anos 2000 com uma interface brilhante, ecológica e inspirada no clássico estilo Frutiger Aero.",
            Category = ShopCategory.Themes,
            CosmeticType = CosmeticType.BoardTheme,
            Rarity = ItemRarity.Epic,
            Price = 120,
            Icon = "◌",
            PreviewTag = "FRUTIGER AERO",
            PreviewCss = "preview-frutiger-aero",
            IsAvailable = true,
            Metadata = new Dictionary<string, object>
            {
                ["cssClass"] = "theme-frutiger-aero",
                ["themeKey"] = "frutiger-aero"
            }
        }
    ];

    public static IReadOnlyList<GameModeItem> GameModes { get; } =
    [
        new GameModeItem
        {
            Id = "mode_classic", Name = "Clássico",
            Description = "O modo padrão. Sempre disponível.",
            Category = ShopCategory.GameModes, GameModeType = GameModeType.Infinite,
            Rarity = ItemRarity.Common, Price = 0, Icon = "▶", PreviewTag = "PADRÃO", IsAvailable = true
        },
        new GameModeItem
        {
            Id = "mode_infinite", Name = "Infinito",
            Description = "Sem limite de mapa. Testa sua resistência. Em breve.",
            Category = ShopCategory.GameModes, GameModeType = GameModeType.Infinite,
            Rarity = ItemRarity.Rare, Price = 75, Icon = "∞", PreviewTag = "EM BREVE", IsAvailable = false
        },
        new GameModeItem
        {
            Id = "mode_hardcore", Name = "Hardcore",
            Description = "Sem pausa. Morte instantânea. Em breve.",
            Category = ShopCategory.GameModes, GameModeType = GameModeType.Hardcore,
            Rarity = ItemRarity.Epic, Price = 100, Icon = "☠", PreviewTag = "EM BREVE", IsAvailable = false
        },
        new GameModeItem
        {
            Id = "mode_ghost", Name = "Ghost",
            Description = "Atravessa paredes. Nova estratégia. Em breve.",
            Category = ShopCategory.GameModes, GameModeType = GameModeType.Ghost,
            Rarity = ItemRarity.Legendary, Price = 200, Icon = "◎", PreviewTag = "EM BREVE", IsAvailable = false
        }
    ];

    public static IEnumerable<ShopItemBase> GetAll()
        => Cosmetics.Cast<ShopItemBase>()
           .Concat(Items.Cast<ShopItemBase>())
           .Concat(Themes.Cast<ShopItemBase>())
           .Concat(GameModes.Cast<ShopItemBase>());

    public static ShopItemBase? FindById(string id)
        => GetAll().FirstOrDefault(i => i.Id == id);
}
