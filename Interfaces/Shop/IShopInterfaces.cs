using SnakeClaude.DTOs.Shop;
using SnakeClaude.Enums;
using SnakeClaude.Models.Shop;

namespace SnakeClaude.Interfaces.Shop;

/// <summary>Contrato do serviço de moeda Snake Bites.</summary>
public interface ICurrencyService
{
    /// <summary>Saldo atual do jogador.</summary>
    int Balance { get; }

    /// <summary>Converte pontos em Snake Bites e adiciona à carteira.</summary>
    ConversionResultDto ConvertPoints(int points);

    /// <summary>Adiciona Snake Bites diretamente (eventos, bônus).</summary>
    void AddCurrency(int amount, string reason);

    /// <summary>Verifica se há saldo suficiente para uma compra.</summary>
    bool CanAfford(int amount);

    /// <summary>Tenta debitar Snake Bites. Retorna false se insuficiente.</summary>
    bool SpendCurrency(int amount, string reason);

    /// <summary>Retorna DTO completo da carteira.</summary>
    WalletDto GetWallet();

    /// <summary>Persiste o estado no LocalStorage.</summary>
    Task SaveAsync();

    /// <summary>Restaura o estado do LocalStorage.</summary>
    Task LoadAsync();

    /// <summary>Disparado sempre que o saldo muda.</summary>
    event EventHandler<int>? OnBalanceChanged;
}

/// <summary>Contrato do serviço da loja.</summary>
public interface IShopService
{
    /// <summary>Retorna todos os itens de uma categoria com estado do jogador.</summary>
    IReadOnlyList<ShopItemDto> GetItemsByCategory(ShopCategory category);

    /// <summary>Garante que os itens gratuitos padrão existam no inventário.</summary>
    void EnsureDefaults();

    /// <summary>Tenta comprar um item. Retorna resultado da operação.</summary>
    Task<PurchaseResultDto> PurchaseAsync(string itemId);

    /// <summary>Equipa ou desequipa um item.</summary>
    void ToggleEquip(string itemId);

    /// <summary>Estado do inventário.</summary>
    PlayerInventory Inventory { get; }

    /// <summary>Persiste inventário.</summary>
    Task SaveInventoryAsync();

    /// <summary>Restaura inventário.</summary>
    Task LoadInventoryAsync();

    /// <summary>Retorna o item de tema visual equipado, se houver.</summary>
    string? GetEquippedThemeId();

    /// <summary>Retorna a classe CSS do tema visual equipado, se houver.</summary>
    string? GetActiveThemeCssClass();

    /// <summary>Disparado quando inventário muda.</summary>
    event EventHandler? OnInventoryChanged;
}
