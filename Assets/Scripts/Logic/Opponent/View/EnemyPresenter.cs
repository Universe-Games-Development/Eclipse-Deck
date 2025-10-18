using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;


public class EnemyPresenter : OpponentPresenter {
    public EnemyPresenter(Opponent opponent, OpponentView opponentView) : base(opponent, opponentView) {
    }
}

public interface IAIController : ITargetSelectionService {
    void StartTurn();
    void EndTurn();
    UniTask ExecuteTurnAsync(CancellationToken cancellationToken = default);
}

public class AIController : IAIController, IDisposable {
    private readonly Opponent _aiOpponent;
    private readonly ITargetFiller _targetFiller;
    private readonly IOperationManager _operationManager;
    private readonly IOpponentRegistry _opponentRegistry;
    private readonly IUnitRegistry _unitRegistry;
    private readonly ILogger _logger;
    private readonly IEventBus<IEvent> _eventBus;

    // Контекст поточної операції для вибору цілей
    private Card _currentCard;

    public event Action<TargetSelectionRequest> OnSelectionStarted;
    public event Action<TargetSelectionRequest, UnitModel> OnSelectionCompleted;
    public event Action<TargetSelectionRequest> OnSelectionCancelled;

    public AIController(
        Opponent aiOpponent,
        ITargetFiller targetFiller,
        IOperationManager operationManager,
        IOpponentRegistry opponentRegistry,
        IUnitRegistry unitRegistry,
        IEventBus<IEvent> eventBus,
        ILogger logger = null) {

        _aiOpponent = aiOpponent ?? throw new ArgumentNullException(nameof(aiOpponent));
        _targetFiller = targetFiller ?? throw new ArgumentNullException(nameof(targetFiller));
        _operationManager = operationManager ?? throw new ArgumentNullException(nameof(operationManager));
        _opponentRegistry = opponentRegistry ?? throw new ArgumentNullException(nameof(opponentRegistry));
        _unitRegistry = unitRegistry ?? throw new ArgumentNullException(nameof(unitRegistry));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus)); ;
        _logger = logger;

        eventBus.SubscribeTo<TurnStartEvent>(HandleTurnStartedEvent);
    }

    private void HandleTurnStartedEvent(ref TurnStartEvent eventData) {
        if (eventData.Opponent != _aiOpponent) return;
        StartTurn();
        ExecuteTurnAsync().Forget();
    }

    // ============================================
    // СТРАТЕГІЧНИЙ РІВЕНЬ (керування ходом)
    // ============================================

    public void StartTurn() {
        _logger?.LogInfo($"AI {_aiOpponent.InstanceId} started turn", LogCategory.AI);
        _aiOpponent.RestoreMana();
    }

    public async UniTask ExecuteTurnAsync(CancellationToken cancellationToken = default) {
        _logger?.LogInfo($"AI {_aiOpponent.InstanceId} executing turn", LogCategory.AI);

        // Симулюємо "думання"
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: cancellationToken);

        // Граємо карти поки можемо
        while (CanPlayAnyCard()) {
            var bestPlay = SelectBestCardPlay();

            if (bestPlay == null) {
                _logger?.LogInfo("AI: No valid card play found", LogCategory.AI);
                break;
            }

            await ExecuteCardPlayAsync(bestPlay, cancellationToken);

            // Пауза між картами
            await UniTask.Delay(TimeSpan.FromSeconds(0.8f), cancellationToken: cancellationToken);
        }

        _logger?.LogInfo($"AI {_aiOpponent.InstanceId} finished playing cards", LogCategory.AI);
    }

    public void EndTurn() {
        _logger?.LogInfo($"AI {_aiOpponent.InstanceId} ended turn", LogCategory.AI);
        _aiOpponent.EndTurn();
    }

    private bool CanPlayAnyCard() {
        if (_aiOpponent.Hand.IsEmpty) {
            return false;
        }

        foreach (var card in _aiOpponent.Hand.Cards) {
            if (CanPlayCard(card)) {
                return true;
            }
        }

        return false;
    }

    private bool CanPlayCard(Card card) {
        if (card.Cost.Current > _aiOpponent.Mana.Current) {
            return false;
        }

        return true;
    }

    private AICardPlay SelectBestCardPlay() {
        var validPlays = new List<AICardPlay>();

        foreach (var card in _aiOpponent.Hand.Cards) {
            if (!CanPlayCard(card)) {
                continue;
            }

            var play = new AICardPlay {
                Card = card,
                Score = 20f // Базова оцінка за можливість зіграти карту
            };

            validPlays.Add(play);
        }

        if (validPlays.Count == 0) {
            return null;
        }

        var bestPlay = validPlays.OrderByDescending(p => p.Score).First();

        _logger?.LogInfo(
            $"AI selected card: {bestPlay.Card.UnitName} (score: {bestPlay.Score:F1})",
            LogCategory.AI);

        return bestPlay;
    }

    private async UniTask ExecuteCardPlayAsync(AICardPlay play, CancellationToken cancellationToken) {
        _logger?.LogInfo($"AI playing card: {play.Card.UnitName}", LogCategory.AI);

        try {
            // Зберігаємо контекст поточної карти
            _currentCard = play.Card;

            // Граємо карту - під час виконання операцій
            // TargetFiller автоматично викличе наш SelectTargetAsync
            _aiOpponent.PlayCard(play.Card);

            // Чекаємо завершення операцій
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);

            _currentCard = null;

        } catch (Exception ex) {
            _logger?.LogError($"AI card play failed: {ex.Message}", LogCategory.AI);
            _currentCard = null;
        }
    }

    // ============================================
    // ТАКТИЧНИЙ РІВЕНЬ (ITargetSelectionService)
    // ============================================

    public async UniTask<UnitModel> SelectTargetAsync(
        TargetSelectionRequest request,
        CancellationToken cancellationToken) {
        OnSelectionStarted?.Invoke(request);
        _logger?.LogInfo(
            $"AI selecting target for {request.Target.Key} (card: {_currentCard})",
            LogCategory.AI);

        // Симулюємо "думання" про вибір цілі
        await UniTask.Delay(
            TimeSpan.FromSeconds(UnityEngine.Random.Range(0.3f, 0.8f)),
            cancellationToken: cancellationToken);

        // AI знає контекст - яку карту грає і яку операцію виконує
        var context = new AIDecisionContext {
            CurrentCard = _currentCard,
            AIOpponent = _aiOpponent,
            TargetRequest = request
        };

        // Отримуємо валідні цілі
        var validTargets = GetValidTargets(request.Target, request.Source);

        if (validTargets.Count == 0) {
            _logger?.LogWarning("AI: No valid targets found", LogCategory.AI);
            return null;
        }

        // Оцінюємо та вибираємо найкращу ціль з урахуванням контексту
        var bestTarget = SelectBestTarget(validTargets, context);
        if (bestTarget == null) {
            OnSelectionCancelled?.Invoke(request);
        }
        OnSelectionCompleted?.Invoke(request, bestTarget);
        _logger?.LogInfo($"AI selected target: {bestTarget?.InstanceId}", LogCategory.AI);
        return bestTarget;
    }

    private List<UnitModel> GetValidTargets(TargetInfo targetInfo, UnitModel source) {
        var validTargets = new List<UnitModel>();
        var context = new ValidationContext(source.OwnerId);

        var allUnits = GetAllPotentialTargets(targetInfo);

        foreach (var unit in allUnits) {
            var validationResult = targetInfo.IsValid(unit, context);
            if (validationResult.IsValid) {
                validTargets.Add(unit);
            }
        }

        return validTargets;
    }

    private IEnumerable<UnitModel> GetAllPotentialTargets(TargetInfo targetInfo) {
        var selector = targetInfo.GetTargetSelector();

        switch (selector) {
            case TargetSelector.Opponent:
                var opponent = _opponentRegistry.GetAgainstOpponentId(_aiOpponent.InstanceId);
                if (opponent != null) {
                    yield return opponent;
                }
                break;

            case TargetSelector.Initiator:
                yield return _aiOpponent;
                break;

            case TargetSelector.AllPlayers:
                var allOpponents = _opponentRegistry.GetOpponents();
                foreach (var opp in allOpponents) {
                    yield return opp;
                }
                break;

            default:
                var allUnits = _unitRegistry.GetAllModels<UnitModel>();
                foreach (var unit in allUnits) {
                    yield return unit;
                }
                break;
        }
    }

    private UnitModel SelectBestTarget(List<UnitModel> validTargets, AIDecisionContext context) {
        // AI аналізує цілі базуючись на повному контексті
        var scoredTargets = new List<(UnitModel target, float score)>();

        foreach (var target in validTargets) {
            float score = EvaluateTarget(target, context);
            scoredTargets.Add((target, score));
        }

        // Вибираємо найкращу ціль
        var best = scoredTargets.OrderByDescending(t => t.score).First();

        _logger?.LogInfo(
            $"AI evaluated {scoredTargets.Count} targets, best score: {best.score:F1}",
            LogCategory.AI);

        return best.target;
    }

    private float EvaluateTarget(UnitModel target, AIDecisionContext context) {
        float score = 0f;

        // Якщо це пошкодження
        if (IsOffensiveTarget(context.TargetRequest)) {
            // Обираємо найслабшого ворога
            if (target is IHealthable healthable && target.OwnerId != _aiOpponent.OwnerId) {
                float healthPercent = healthable.CurrentHealth / (float)healthable.BaseValue;
                score += (1f - healthPercent) * 50f; // Чим менше HP, тим краще

                // Якщо можемо вбити - дуже добре
                if (healthable.CurrentHealth <= GetDamageAmount(context.CurrentCard)) {
                    score += 100f;
                }
            }
        }
        // Якщо це лікування
        else if (IsHealingTarget(context.TargetRequest)) {
            // Лікуємо себе якщо поранені
            if (target == _aiOpponent) {
                float healthPercent = _aiOpponent.CurrentHealth / (float)_aiOpponent.Health.TotalValue;
                score += (1f - healthPercent) * 60f; // Чим менше HP, тим важливіше
            }
        }

        // Трохи випадковості
        score += UnityEngine.Random.Range(-5f, 5f);

        return score;
    }

    private bool IsOffensiveTarget(TargetSelectionRequest request) {
        // Перевіряємо чи це атакуюча операція
        return request.Target.GetTargetSelector() == TargetSelector.Opponent;
    }

    private bool IsHealingTarget(TargetSelectionRequest request) {
        return request.Target.GetTargetSelector() == TargetSelector.Initiator;
    }

    private int GetDamageAmount(Card card) {
        // Простий підрахунок пошкоджень (можна покращити)
        if (card == null) return 0;

        return 2;
    }

    public void CancelCurrentSelection() {
        throw new NotImplementedException();
    }

    public void Dispose() {
        _eventBus.UnsubscribeFrom<TurnStartEvent>(HandleTurnStartedEvent);
    }

    private class AICardPlay {
        public Card Card { get; set; }
        public float Score { get; set; }
    }

    private class AIDecisionContext {
        public Card CurrentCard { get; set; }
        public Opponent AIOpponent { get; set; }
        public TargetSelectionRequest TargetRequest { get; set; }
    }
}