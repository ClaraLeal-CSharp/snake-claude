using Microsoft.Extensions.Options;
using SnakeClaude.Configuration;
using SnakeClaude.Enums;
using SnakeClaude.Events;
using SnakeClaude.Interfaces;
using SnakeClaude.Models;
using SnakeClaude.State;

namespace SnakeClaude.Engine;

/// <summary>
/// Engine principal do jogo Snake.
///
/// Responsabilidades:
/// - Gerenciar o game loop (tick-based via PeriodicTimer)
/// - Coordenar os serviços (movimento, comida, pontuação)
/// - Disparar eventos para o front-end (Observer Pattern)
/// - Produzir snapshots imutáveis do estado
///
/// Decisões:
/// - PeriodicTimer é preferível a Timer/Task.Delay pois não acumula ticks perdidos
/// - A engine é Scoped: uma instância por sessão/jogador
/// - Toda mutação de estado ocorre dentro do game loop (thread safety simplificado)
/// - O front-end NUNCA acessa o estado mutável diretamente — apenas via snapshot/eventos
/// </summary>
public sealed class GameEngine(
    IOptions<GameSettings> settings,
    IMovementService movementService,
    IFoodService foodService,
    IScoreService scoreService,
    ILogger<GameEngine> logger) : IGameEngine
{
    private readonly GameSettings _settings = settings.Value;

    // ── Estado mutável interno (não exposto diretamente) ───────────────────
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
        if (_status is GameStatus.Running)
            return;

        logger.LogInformation("Iniciando novo jogo — Grid {W}x{H}", _settings.GridWidth, _settings.GridHeight);

        SubscribeScoreEvents();
        InitializeGame();
        ChangeStatus(GameStatus.Running);
        await StartLoopAsync();
    }

    public async Task PauseAsync()
    {
        if (_status is not GameStatus.Running)
            return;

        await StopLoopAsync();
        ChangeStatus(GameStatus.Paused);
        logger.LogInformation("Jogo pausado no tick {Tick}", _tickCount);
    }

    public async Task ResumeAsync()
    {
        if (_status is not GameStatus.Paused)
            return;

        ChangeStatus(GameStatus.Running);
        await StartLoopAsync();
        logger.LogInformation("Jogo retomado");
    }

    public async Task ResetAsync()
    {
        await StopLoopAsync();
        scoreService.Reset();
        _activeFoods.Clear();
        _tickCount = 0;
        ChangeStatus(GameStatus.Idle);
        logger.LogInformation("Jogo resetado");
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
        catch (OperationCanceledException)
        {
            logger.LogDebug("Game loop cancelado");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado no game loop");
        }
    }

    // ── Lógica de Tick ────────────────────────────────────────────────────

    private void ProcessTick()
    {
        if (_snake is null || _grid is null || _status is not GameStatus.Running)
            return;

        _tickCount++;

        // 1. Verificar timeout de combo
        scoreService.CheckComboExpiry();

        // 2. Calcular próxima posição da cabeça
        var nextHead = movementService.CalculateNextHead(
            _snake.Head,
            _snake.NextDirection,
            _grid,
            _settings.WallMode);

        // 3. Verificar colisão com parede
        if (movementService.IsWallCollision(nextHead, _grid, _settings.WallMode))
        {
            TriggerGameOver("Colisão com a parede");
            return;
        }

        // 4. Verificar se é comida ANTES de mover (para lógica de crescimento)
        var collectedFood = _activeFoods.FirstOrDefault(f => f.Position == nextHead);
        bool isEating = collectedFood is not null;

        // 5. Verificar auto-colisão (exceto se vai comer — cauda se moverá)
        if (!isEating && movementService.IsSelfCollision(nextHead, _snake))
        {
            TriggerGameOver("Auto-colisão");
            return;
        }

        // 6. Mover cobra (grow=true se comeu)
        _snake.Move(nextHead, grow: isEating);

        // 7. Processar coleta de comida
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

            // Aumentar velocidade progressivamente
            AdjustSpeed();

            // Verificar vitória (grid cheio)
            if (_snake.Length >= _grid.TotalCells)
            {
                TriggerVictory();
                return;
            }

            // Spawnar nova comida se necessário
            EnsureFoodCount();
        }

        // 8. Disparar evento de tick com snapshot
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

        // Posição inicial: corpo alinhado para a esquerda, cabeça movendo para a direita.
        var initialLength = Math.Clamp(_settings.InitialSnakeLength, 1, _settings.GridWidth);
        var startX = Math.Clamp(_settings.GridWidth / 2, initialLength - 1, _settings.GridWidth - 1);
        var startY = _settings.GridHeight / 2;
        var startPos = new Position(startX, startY);

        var tailStart = new Position(startX - initialLength + 1, startY);
        _snake = new Snake(tailStart, Direction.Right);

        // Pré-popular corpo inicial da cauda até a cabeça.
        for (int x = tailStart.X + 1; x <= startX; x++)
            _snake.Move(new Position(x, startY), grow: true);

        // Spawnar comida inicial
        EnsureFoodCount();

        logger.LogInformation("Jogo inicializado — Snake em {Pos}, {FoodCount} comida(s)", startPos, _activeFoods.Count);
    }

    private void EnsureFoodCount()
    {
        if (_snake is null || _grid is null) return;

        while (_activeFoods.Count < _settings.MaxFoodItems)
        {
            var occupied = _snake.Body.Concat(_activeFoods.Select(f => f.Position));
            var food = foodService.SpawnFood(_grid, occupied);

            if (food.Position.X < 0) break; // Grid lotado

            food = food with { Position = food.Position }; // Preservar record imutável com pontos configurados
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

        // Recriar timer com novo intervalo
        _timer?.Dispose();
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_currentTickIntervalMs));
        logger.LogDebug("Velocidade ajustada: {Interval}ms por tick", _currentTickIntervalMs);
    }

    // ── Game Over / Vitória ────────────────────────────────────────────────

    private void TriggerGameOver(string reason)
    {
        logger.LogInformation("Game Over: {Reason} | Score: {Score}", reason, scoreService.ScoreBoard.TotalScore);
        ChangeStatus(GameStatus.GameOver);
        var snapshot = BuildSnapshot(reason);
        OnGameOver?.Invoke(this, new GameOverEventArgs(snapshot, reason));
        _ = StopLoopAsync();
    }

    private void TriggerVictory()
    {
        logger.LogInformation("Vitória! Score final: {Score}", scoreService.ScoreBoard.TotalScore);
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
        if (_scoreEventsSubscribed)
            return;

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
        logger.LogDebug("GameEngine disposed");
    }
}
