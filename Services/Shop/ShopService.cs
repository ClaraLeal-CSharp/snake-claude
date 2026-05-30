using Microsoft.JSInterop;
using SnakeClaude.DTOs.Shop;
using SnakeClaude.Enums;
using SnakeClaude.Interfaces.Shop;
using SnakeClaude.Models.Shop;
using System.Text.Json;

namespace SnakeClaude.Services.Shop;

/// <summary>
/// Serviço da loja — gerencia catálogo, compras e inventário.
/// </summary>
public class ShopService(IJSRuntime js, ICurrencyService currency) : IShopService
{
    private readonly IJSRuntime _js = js;
    private readonly ICurrencyService _currency = currency;

    private const string InventoryKey = "snakeclaude:inventory";

    public PlayerInventory Inventory { get; } = new();

    public event EventHandler? OnInventoryChanged;

    // ── Inicialização: garante que itens gratuitos já estão no inventário ──
    public void EnsureDefaults()
    {
        foreach (var item in ShopCatalog.GetAll().Where(i => i.Price == 0 && i.IsAvailable))
        {
            if (!Inventory.HasItem(item.Id))
            {
                var equip = item.Id is "skin_default" or "mode_classic";
                Inventory.AddItem(item.Id, item.Category, setEquipped: equip);
            }
        }
    }

    // ── Catálogo com estado do jogador ────────────────────────────
    public IReadOnlyList<ShopItemDto> GetItemsByCategory(ShopCategory category)
    {
        var catalogItems = category switch
        {
            ShopCategory.Cosmetics  => ShopCatalog.Cosmetics.Cast<ShopItemBase>(),
            ShopCategory.Items      => ShopCatalog.Items.Cast<ShopItemBase>(),
            ShopCategory.GameModes  => ShopCatalog.GameModes.Cast<ShopItemBase>(),
            ShopCategory.Themes     => ShopCatalog.Themes.Cast<ShopItemBase>(),
            _                       => Enumerable.Empty<ShopItemBase>()
        };

        return catalogItems.Select(item => new ShopItemDto
        {
            Id          = item.Id,
            Name        = item.Name,
            Description = item.Description,
            Category    = item.Category,
            Rarity      = item.Rarity,
            Price       = item.Price,
            Icon        = item.Icon,
            PreviewTag  = item.PreviewTag,
            IsAvailable = item.IsAvailable,
            State       = ResolveState(item)
        }).ToList();
    }

    private ShopItemState ResolveState(ShopItemBase item)
    {
        if (!item.IsAvailable)             return ShopItemState.Unavailable;
        if (Inventory.HasItem(item.Id))    return Inventory.GetItemState(item.Id);
        return ShopItemState.Available;
    }

    // ── Compra ────────────────────────────────────────────────────
    public async Task<PurchaseResultDto> PurchaseAsync(string itemId)
    {
        var item = ShopCatalog.FindById(itemId);
        if (item is null)
            return Fail("Item não encontrado.");

        if (!item.IsAvailable)
            return Fail("Item indisponível.");

        if (Inventory.HasItem(itemId))
            return Fail("Item já adquirido.");

        if (item.Price > 0 && !_currency.CanAfford(item.Price))
            return Fail($"Saldo insuficiente. Necessário: {item.Price} Snake Bites.");

        if (item.Price > 0)
            _currency.SpendCurrency(item.Price, $"Compra: {item.Name}");

        Inventory.AddItem(itemId, item.Category);
        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
        await SaveInventoryAsync();
        await _currency.SaveAsync();

        return new PurchaseResultDto
        {
            Success    = true,
            Message    = $"{item.Name} adquirido!",
            NewBalance = _currency.Balance,
            ItemId     = itemId
        };
    }

    // ── Equipar / Desequipar ──────────────────────────────────────
    public void ToggleEquip(string itemId)
    {
        var entry = Inventory.GetEntry(itemId);
        if (entry is null) return;

        if (entry.State == ShopItemState.Equipped)
            Inventory.UnequipItem(itemId);
        else
            Inventory.EquipItem(itemId);

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
        _ = SaveInventoryAsync();
    }

    public string? GetEquippedThemeId()
    {
        var equippedThemeIds = ShopCatalog.Themes.Select(theme => theme.Id).ToHashSet();
        return Inventory.GetEquipped()
            .FirstOrDefault(entry => equippedThemeIds.Contains(entry.ItemId))
            ?.ItemId;
    }

    public string? GetActiveThemeCssClass()
    {
        var themeId = GetEquippedThemeId();
        if (themeId is null) return null;

        var theme = ShopCatalog.Themes.FirstOrDefault(item => item.Id == themeId);
        return theme?.Metadata.TryGetValue("cssClass", out var cssClass) == true
            ? cssClass?.ToString()
            : null;
    }

    // ── Persistência ──────────────────────────────────────────────
    public async Task SaveInventoryAsync()
    {
        try
        {
            var data = JsonSerializer.Serialize(
                Inventory.Entries.Select(e => new InventoryPersisted(
                    e.ItemId, (int)e.Category, (int)e.State,
                    e.Quantity, e.AcquiredAt)).ToList());
            await _js.InvokeVoidAsync("localStorage.setItem", InventoryKey, data);
        }
        catch { }
    }

    public async Task LoadInventoryAsync()
    {
        try
        {
            var raw = await _js.InvokeAsync<string?>("localStorage.getItem", InventoryKey);
            if (string.IsNullOrEmpty(raw)) return;
            var list = JsonSerializer.Deserialize<List<InventoryPersisted>>(raw);
            if (list is null) return;

            foreach (var p in list)
            {
                if (!Inventory.HasItem(p.ItemId))
                    Inventory.AddItem(p.ItemId, (ShopCategory)p.Category, p.Quantity);

                var entry = Inventory.GetEntry(p.ItemId);
                if (entry is not null)
                {
                    entry.State     = (ShopItemState)p.State;
                    entry.Quantity  = p.Quantity;
                    entry.AcquiredAt = p.AcquiredAt;
                }
            }
            OnInventoryChanged?.Invoke(this, EventArgs.Empty);
        }
        catch { }
    }

    private static PurchaseResultDto Fail(string msg)
        => new() { Success = false, Message = msg };

    private record InventoryPersisted(
        string ItemId, int Category, int State, int Quantity, DateTime AcquiredAt);
}
