using Microsoft.JSInterop;
using SnakeClaude.DTOs.Shop;
using SnakeClaude.Interfaces.Shop;
using SnakeClaude.Models.Shop;
using System.Text.Json;

namespace SnakeClaude.Services.Shop;

/// <summary>
/// Serviço de moeda Snake Bites.
/// Regra de conversão: 50 pontos = 1 Snake Bite (arredondamento para baixo).
/// </summary>
public class CurrencyService(IJSRuntime js) : ICurrencyService
{
    private readonly IJSRuntime _js = js;
    private readonly PlayerWallet _wallet = new();

    private const int PointsPerBite = 50;
    private const string StorageKey = "snakeclaude:wallet";

    public int Balance => _wallet.Balance;

    public event EventHandler<int>? OnBalanceChanged;

    // ── Conversão de pontos ───────────────────────────────────────
    public ConversionResultDto ConvertPoints(int points)
    {
        if (points <= 0)
            return new ConversionResultDto { DisplayText = "+0 Snake Bites" };

        var bites = points / PointsPerBite;
        if (bites <= 0)
            return new ConversionResultDto
            {
                PointsConverted  = points,
                SnakeBitesEarned = 0,
                NewBalance       = _wallet.Balance,
                DisplayText      = "+0 Snake Bites"
            };

        _wallet.Add(bites, $"Partida: {points} pts convertidos");
        OnBalanceChanged?.Invoke(this, _wallet.Balance);

        return new ConversionResultDto
        {
            PointsConverted  = points,
            SnakeBitesEarned = bites,
            NewBalance       = _wallet.Balance,
            DisplayText      = $"+{bites} Snake Bite{(bites != 1 ? "s" : "")}"
        };
    }

    public void AddCurrency(int amount, string reason)
    {
        if (amount <= 0) return;
        _wallet.Add(amount, reason);
        OnBalanceChanged?.Invoke(this, _wallet.Balance);
    }

    public bool CanAfford(int amount) => _wallet.CanAfford(amount);

    public bool SpendCurrency(int amount, string reason)
    {
        var ok = _wallet.Spend(amount, reason);
        if (ok) OnBalanceChanged?.Invoke(this, _wallet.Balance);
        return ok;
    }

    public WalletDto GetWallet()
    {
        return new WalletDto
        {
            Balance     = _wallet.Balance,
            TotalEarned = _wallet.TotalEarned,
            TotalSpent  = _wallet.TotalSpent,
            RecentHistory = _wallet.History.Take(10).Select(t => new TransactionDto
            {
                Delta     = t.Delta,
                Reason    = t.Reason,
                TypeLabel = t.Type switch
                {
                    TransactionType.Earn   => "GANHO",
                    TransactionType.Spend  => "GASTO",
                    TransactionType.Refund => "REEMBOLSO",
                    _                      => "—"
                },
                TimeAgo = FormatTimeAgo(t.Timestamp)
            }).ToList()
        };
    }

    // ── Persistência ──────────────────────────────────────────────
    public async Task SaveAsync()
    {
        try
        {
            var data = JsonSerializer.Serialize(new WalletPersisted
            {
                Balance     = _wallet.Balance,
                TotalEarned = _wallet.TotalEarned,
                TotalSpent  = _wallet.TotalSpent
            });
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, data);
        }
        catch { /* silencioso em WASM */ }
    }

    public async Task LoadAsync()
    {
        try
        {
            var raw = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrEmpty(raw)) return;
            var data = JsonSerializer.Deserialize<WalletPersisted>(raw);
            if (data is null) return;
            _wallet.Restore(data.Balance, data.TotalEarned, data.TotalSpent);
            OnBalanceChanged?.Invoke(this, _wallet.Balance);
        }
        catch { }
    }

    private static string FormatTimeAgo(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 1)  return "agora";
        if (diff.TotalHours   < 1)  return $"{(int)diff.TotalMinutes}min atrás";
        if (diff.TotalDays    < 1)  return $"{(int)diff.TotalHours}h atrás";
        return $"{(int)diff.TotalDays}d atrás";
    }

    private record WalletPersisted(int Balance, int TotalEarned, int TotalSpent)
    {
        public WalletPersisted() : this(0, 0, 0) { }
    }
}
