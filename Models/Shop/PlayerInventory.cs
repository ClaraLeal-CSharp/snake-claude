using SnakeClaude.Enums;

namespace SnakeClaude.Models.Shop;

/// <summary>
/// Registro de posse/estado de um item pelo jogador.
/// </summary>
public class InventoryEntry
{
    public string ItemId          { get; set; } = string.Empty;
    public ShopCategory Category  { get; set; }
    public ShopItemState State    { get; set; } = ShopItemState.Purchased;
    public int Quantity           { get; set; } = 1;      // Para consumíveis
    public DateTime AcquiredAt    { get; set; } = DateTime.UtcNow;
    public DateTime? EquippedAt   { get; set; }
}

/// <summary>
/// Inventário completo do jogador.
/// </summary>
public class PlayerInventory
{
    public List<InventoryEntry> Entries { get; set; } = [];

    public bool HasItem(string itemId)
        => Entries.Any(e => e.ItemId == itemId);

    public InventoryEntry? GetEntry(string itemId)
        => Entries.FirstOrDefault(e => e.ItemId == itemId);

    public ShopItemState GetItemState(string itemId)
        => GetEntry(itemId)?.State ?? ShopItemState.Available;

    /// <summary>Adiciona item ao inventário.</summary>
    public void AddItem(string itemId, ShopCategory category,
                        int quantity = 1, bool setEquipped = false)
    {
        var existing = GetEntry(itemId);
        if (existing is not null)
        {
            existing.Quantity += quantity;
            return;
        }
        Entries.Add(new InventoryEntry
        {
            ItemId     = itemId,
            Category   = category,
            State      = setEquipped ? ShopItemState.Equipped : ShopItemState.Purchased,
            Quantity   = quantity,
            AcquiredAt = DateTime.UtcNow
        });
    }

    /// <summary>Equipa um item (desequipa outros da mesma categoria se exclusivo).</summary>
    public void EquipItem(string itemId, bool exclusive = true)
    {
        var entry = GetEntry(itemId);
        if (entry is null || entry.State == ShopItemState.Equipped) return;

        if (exclusive)
        {
            var sameCategory = Entries.Where(e => e.Category == entry.Category
                                               && e.State == ShopItemState.Equipped);
            foreach (var e in sameCategory)
            {
                e.State     = ShopItemState.Purchased;
                e.EquippedAt = null;
            }
        }

        entry.State     = ShopItemState.Equipped;
        entry.EquippedAt = DateTime.UtcNow;
    }

    /// <summary>Desequipa um item.</summary>
    public void UnequipItem(string itemId)
    {
        var entry = GetEntry(itemId);
        if (entry is null) return;
        entry.State      = ShopItemState.Purchased;
        entry.EquippedAt = null;
    }

    /// <summary>Consome 1 unidade de um consumível. Retorna false se não há saldo.</summary>
    public bool ConsumeItem(string itemId)
    {
        var entry = GetEntry(itemId);
        if (entry is null || entry.Quantity <= 0) return false;
        entry.Quantity--;
        if (entry.Quantity == 0)
            entry.State = ShopItemState.Purchased; // esgotado mas mantém histórico
        return true;
    }

    public IEnumerable<InventoryEntry> GetEquipped()
        => Entries.Where(e => e.State == ShopItemState.Equipped);

    public IEnumerable<InventoryEntry> ByCategory(ShopCategory category)
        => Entries.Where(e => e.Category == category);
}
