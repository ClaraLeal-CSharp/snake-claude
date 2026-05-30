using SnakeClaude.Enums;
using SnakeClaude.Models.Shop;

namespace SnakeClaude.DTOs.Shop;

/// <summary>DTO de item da loja para exibição no front-end.</summary>
public class ShopItemDto
{
    public string Id          { get; set; } = string.Empty;
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ShopCategory Category { get; set; }
    public ItemRarity Rarity  { get; set; }
    public int Price          { get; set; }
    public string Icon        { get; set; } = "▪";
    public string? PreviewTag { get; set; }
    public bool IsAvailable   { get; set; }
    public ShopItemState State { get; set; }
    public string RarityLabel => Rarity switch
    {
        ItemRarity.Common    => "COMUM",
        ItemRarity.Rare      => "RARO",
        ItemRarity.Epic      => "ÉPICO",
        ItemRarity.Legendary => "LENDÁRIO",
        _                    => "COMUM"
    };
    public string RarityCss => Rarity switch
    {
        ItemRarity.Common    => "rarity-common",
        ItemRarity.Rare      => "rarity-rare",
        ItemRarity.Epic      => "rarity-epic",
        ItemRarity.Legendary => "rarity-legendary",
        _                    => "rarity-common"
    };
}

/// <summary>DTO de resultado de compra.</summary>
public class PurchaseResultDto
{
    public bool Success       { get; set; }
    public string Message     { get; set; } = string.Empty;
    public int NewBalance     { get; set; }
    public string? ItemId     { get; set; }
}

/// <summary>DTO da carteira para o front-end.</summary>
public class WalletDto
{
    public int Balance        { get; set; }
    public int TotalEarned    { get; set; }
    public int TotalSpent     { get; set; }
    public List<TransactionDto> RecentHistory { get; set; } = [];
}

/// <summary>DTO de transação para exibição.</summary>
public class TransactionDto
{
    public int Delta          { get; set; }
    public string Reason      { get; set; } = string.Empty;
    public string TypeLabel   { get; set; } = string.Empty;
    public string TimeAgo     { get; set; } = string.Empty;
}

/// <summary>DTO de resultado de conversão de pontos em Snake Bites.</summary>
public class ConversionResultDto
{
    public int PointsConverted { get; set; }
    public int SnakeBitesEarned { get; set; }
    public int NewBalance      { get; set; }
    public string DisplayText  { get; set; } = string.Empty;
}
