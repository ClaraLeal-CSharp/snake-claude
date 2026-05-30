using SnakeClaude.Enums;

namespace SnakeClaude.Models.Shop;

/// <summary>
/// Carteira do jogador — saldo e histórico de Snake Bites.
/// </summary>
public class PlayerWallet
{
    /// <summary>Saldo atual de Snake Bites.</summary>
    public int Balance { get; private set; }

    /// <summary>Total acumulado de Snake Bites ganhos (lifetime).</summary>
    public int TotalEarned { get; private set; }

    /// <summary>Total gasto em Snake Bites.</summary>
    public int TotalSpent { get; private set; }

    /// <summary>Histórico simplificado de transações (últimas 50).</summary>
    public List<WalletTransaction> History { get; private set; } = [];

    private const int MaxHistoryEntries = 50;

    /// <summary>Adiciona Snake Bites à carteira.</summary>
    public void Add(int amount, string reason)
    {
        if (amount <= 0) return;
        Balance     += amount;
        TotalEarned += amount;
        AddHistory(amount, reason, TransactionType.Earn);
    }

    /// <summary>Debita Snake Bites da carteira. Retorna false se saldo insuficiente.</summary>
    public bool Spend(int amount, string reason)
    {
        if (amount <= 0 || Balance < amount) return false;
        Balance    -= amount;
        TotalSpent += amount;
        AddHistory(-amount, reason, TransactionType.Spend);
        return true;
    }

    public bool CanAfford(int amount) => Balance >= amount;

    private void AddHistory(int delta, string reason, TransactionType type)
    {
        History.Insert(0, new WalletTransaction
        {
            Delta     = delta,
            Reason    = reason,
            Type      = type,
            Timestamp = DateTime.UtcNow
        });
        if (History.Count > MaxHistoryEntries)
            History.RemoveAt(History.Count - 1);
    }

    /// <summary>Restaura estado a partir de dados persistidos.</summary>
    public void Restore(int balance, int totalEarned, int totalSpent,
                        List<WalletTransaction>? history = null)
    {
        Balance     = Math.Max(0, balance);
        TotalEarned = Math.Max(0, totalEarned);
        TotalSpent  = Math.Max(0, totalSpent);
        History     = history ?? [];
    }
}

public class WalletTransaction
{
    public int Delta              { get; init; }
    public string Reason          { get; init; } = string.Empty;
    public TransactionType Type   { get; init; }
    public DateTime Timestamp     { get; init; }
}

public enum TransactionType { Earn, Spend, Refund }
