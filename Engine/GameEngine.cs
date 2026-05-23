using SnakeClaude.Configuration;
using SnakeClaude.Enums;
using SnakeClaude.Events;
using SnakeClaude.Interfaces;
using SnakeClaude.Models;
using SnakeClaude.State;

namespace SnakeClaude.Engine;

/// <summary>
/// Engine principal do jogo Snake — compatível com Blazor WASM.
/// Recebe GameSettings diretamente (sem IOptions) e não usa ILogger (WASM-friendly).
/// </summary>
public sealed class GameEngine(
    GameSettings settings,
    IMovementService movementService,
    IFoodService foodService,
    IScoreService scoreService) : IGameEngine
{
    private readonly GameSettings _settings = settings;

    // ── Estado mutável interno ─────────────────────────────────────────────
    private Snake? _snake;
    private GameGrid? _grid;
    private readonly List<Food> _activeFoods = [];
    private GameStatus _status = GameStatus.Idle;
    private long _tickCount;
    private int _currentTickIntervalMs;
    private bool _scoreEventsSubscribed;

    // ── Game Loop ──────────────────────────────────────────────────────────
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    // ── Eventos públicos ───────────────────────────────────────────────────
    public event EventHandler<GameTickEventArgs>? OnTick;
    public event EventHandler<FoodCollectedEventArgs>? OnFoodCollected;
    public event EventHandler<GameStatusChangedEventArgs>? OnStatusChanged;
    public event EventHandler<GameOverEventArgs>? OnGameOver;
    public event EventHandler<FoodSpawnedEventArgs>? OnFoodSpawned;
    public event EventHandler<ComboChangedEventArgs>? OnComboChanged;
    public event EventHandler<ComboResetEventArgs>? OnComboReset;

    // ── Interface pública ──────────────────────────────────────────────────
    public GameStatus Status => _status;
    public GameStateSnapshot CurrentSnapshot => BuildSnapshot();

    // ── Controle do Jogo ───────────────────────────────────────────────────
    public async Task StartAsync()
    {
        if (_status is GameStatus.Running) return;
        SubscribeScoreEvents();
        InitializeGame();
        ChangeStatus(GameStatus.Running);
        await StartLoopAsync();
    }

    public async Task PauseAsync()
    {
        if (_status is not GameStatus.Running) return;
        await StopLoopAsync();
        ChangeStatus(GameStatus.Paused);
    }

    public async Task ResumeAsync()
    {
        if (_status is not GameStatus.Paused) return;
        ChangeStatus(GameStatus.Running);
        await StartLoopAsync();
    }

    public async Task ResetAsync()
    {
        await StopLoopAsync();
        scoreService.Reset();
        _activeFoods.Clear();
        _tickCount = 0;
        ChangeStatus(GameStatus.Idle);
    }

    public void RequestDirectionChange(Direction direction)
    {
        _snake?.RequestDirectionChange(direction);
    }

    // ── Game Loop ──────────────────────────────────────────────────────────
    private async Task StartLoopAsync()
    {
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_currentTickIntervalMs));
        _loopTask = RunLoopAsync(_cts.Token);
        await Task.CompletedTask;
    }

    private async Task StopLoopAsync()
    {
        if (_cts is null) return;
        await _cts.CancelAsync();
        _timer?.Dispose();
        if (_loopTask is not null)
            await _loopTask.ConfigureAwait(false);
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _timer is not null)
            {
                if (!await _timer.WaitForNextTickAsync(ct))
                    break;
                ProcessTick();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GameEngine] Erro no game loop: {ex.Message}");
        }
    }

    // ── Lógica de Tick ────────────────────────────────────────────────────
    private void ProcessTick()
    {
        if (_snake is null || _grid is null || _status is not GameStatus.Running) return;

        _tickCount++;
        scoreService.CheckComboExpiry();

        var nextHead = movementService.CalculateNextHead(
            _snake.Head, _snake.NextDirection, _grid, _settings.WallMode);

        if (movementService.IsWallCollision(nextHead, _grid, _settings.WallMode))
        {
            TriggerGameOver("Colisão com a parede");
            return;
        }

        var collectedFood = _activeFoods.FirstOrDefault(f => f.Position == nextHead);
        bool isEating = collectedFood is not null;

        if (!isEating && movementService.IsSelfCollision(nextHead, _snake))
        {
            TriggerGameOver("Auto-colisão");
            return;
        }

        _snake.Move(nextHead, grow: isEating);

        if (isEating && collectedFood is not null)
        {
            _activeFoods.Remove(collectedFood);
            var basePoints = foodService.Collect(collectedFood);
            var earned = scoreService.ProcessFoodCollection(basePoints);

            OnFoodCollected?.Invoke(this, new FoodCollectedEventArgs(
                collectedFood.Position,
                earned,
                scoreService.ScoreBoard.ComboMultiplier,
                scoreService.ScoreBoard.ComboCount));

            AdjustSpeed();

            if (_snake.Length >= _grid.TotalCells)
            {
                TriggerVictory();
                return;
            }

            EnsureFoodCount();
        }

        var snapshot = BuildSnapshot();
        OnTick?.Invoke(this, new GameTickEventArgs(snapshot));
    }

    // ── Inicialização ─────────────────────────────────────────────────────
    private void InitializeGame()
    {
        _grid = new GameGrid(_settings.GridWidth, _settings.GridHeight);
        _currentTickIntervalMs = _settings.TickIntervalMs;
        _activeFoods.Clear();
        _tickCount = 0;

        var initialLength = Math.Clamp(_settings.InitialSnakeLength, 1, _settings.GridWidth);
        var startX = Math.Clamp(_settings.GridWidth / 2, initialLength - 1, _settings.GridWidth - 1);
        var startY = _settings.GridHeight / 2;

        var tailStart = new Position(startX - initialLength + 1, startY);
        _snake = new Snake(tailStart, Direction.Right);

        for (int x = tailStart.X + 1; x <= startX; x++)
            _snake.Move(new Position(x, startY), grow: true);

        EnsureFoodCount();
    }

    private void EnsureFoodCount()
    {
        if (_snake is null || _grid is null) return;

        while (_activeFoods.Count < _settings.MaxFoodItems)
        {
            var occupied = _snake.Body.Concat(_activeFoods.Select(f => f.Position));
            var food = foodService.SpawnFood(_grid, occupied);

            if (food.Position.X < 0) break;

            var configuredFood = Food.Create(food.Position, _settings.BasePointsPerFood);
            _activeFoods.Add(configuredFood);

            OnFoodSpawned?.Invoke(this, new FoodSpawnedEventArgs(configuredFood.Position, configuredFood.Points));
        }
    }

    private void AdjustSpeed()
    {
        _currentTickIntervalMs = Math.Max(
            _settings.MinTickIntervalMs,
            _currentTickIntervalMs - _settings.SpeedIncrementMs);

        _timer?.Dispose();
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_currentTickIntervalMs));
    }

    // ── Game Over / Vitória ────────────────────────────────────────────────
    private void TriggerGameOver(string reason)
    {
        ChangeStatus(GameStatus.GameOver);
        var snapshot = BuildSnapshot(reason);
        OnGameOver?.Invoke(this, new GameOverEventArgs(snapshot, reason));
        _ = StopLoopAsync();
    }

    private void TriggerVictory()
    {
        ChangeStatus(GameStatus.Victory);
        var snapshot = BuildSnapshot("Vitória — grid completo!");
        OnGameOver?.Invoke(this, new GameOverEventArgs(snapshot, "Vitória"));
        _ = StopLoopAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private void ChangeStatus(GameStatus newStatus)
    {
        var previous = _status;
        _status = newStatus;
        OnStatusChanged?.Invoke(this, new GameStatusChangedEventArgs(previous, newStatus));
    }

    private void SubscribeScoreEvents()
    {
        if (_scoreEventsSubscribed) return;
        scoreService.OnComboChanged += HandleComboChanged;
        scoreService.OnComboReset += HandleComboReset;
        _scoreEventsSubscribed = true;
    }

    private void HandleComboChanged(object? sender, ComboChangedEventArgs e)
        => OnComboChanged?.Invoke(this, e);

    private void HandleComboReset(object? sender, ComboResetEventArgs e)
        => OnComboReset?.Invoke(this, e);

    private GameStateSnapshot BuildSnapshot(string? gameOverReason = null) => new()
    {
        Status = _status,
        SnakeBody = _snake?.GetBodySnapshot() ?? [],
        CurrentDirection = _snake?.CurrentDirection ?? Direction.Right,
        FoodPositions = _activeFoods.Select(f => f.Position).ToList(),
        Score = scoreService.ScoreBoard.ToSnapshot(),
        GridWidth = _grid?.Width ?? _settings.GridWidth,
        GridHeight = _grid?.Height ?? _settings.GridHeight,
        TickCount = _tickCount,
        Timestamp = DateTime.UtcNow,
        GameOverReason = gameOverReason,
        WallMode = _settings.WallMode,
        CurrentTickIntervalMs = _currentTickIntervalMs
    };

    // ── Dispose ───────────────────────────────────────────────────────────
    public async ValueTask DisposeAsync()
    {
        if (_scoreEventsSubscribed)
        {
            scoreService.OnComboChanged -= HandleComboChanged;
            scoreService.OnComboReset -= HandleComboReset;
            _scoreEventsSubscribed = false;
        }

        await StopLoopAsync();
        _cts?.Dispose();
        _timer?.Dispose();
    }
}
