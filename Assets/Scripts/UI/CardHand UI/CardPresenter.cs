using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public class CardPresenter : UnitPresenter {
    public Action<CardPresenter> OnCardClicked;
    public Action<CardPresenter, bool> OnCardHovered;
    public Card Card { get; private set; }
    public CardView View { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool DoTestRandomUpdate = true;
    [SerializeField] private int updateTimes = 15;
    [SerializeField] private float updateRate = 1.0f;
    [SerializeField] private bool _isInteractable = true;

    protected override void OnDestroy() {
        base.OnDestroy();
        UnSubscribeEvents();
    }

    public void Initialize(Card card, CardView cardView) {
        this.Card = card;
        View = cardView;

        SubscribeEvents();
        UpdateUIInfo();

        if (DoTestRandomUpdate) {
            LaunchTestUpdate().Forget();
        }
    }

    public async UniTask LaunchTestUpdate() {
        for (int i = 0; i < updateTimes; i++) {
            int randomCost = UnityEngine.Random.Range(1, 100);
            Card.Cost.Add(randomCost);
            Debug.Log($"Iteration {i}, {randomCost}");
            await UniTask.Delay(TimeSpan.FromSeconds(updateRate));
        }
    }

    private void SubscribeEvents() {
        if (View == null) return;
        View.OnCardClicked += (view) => OnCardClicked?.Invoke(this);
        View.OnHoverChanged += (view, isHovered) => OnCardHovered?.Invoke(this, isHovered);
    }
    private void UnSubscribeEvents() {
        if (View == null) return;
        View.OnCardClicked -= (view) => OnCardClicked?.Invoke(this);
        View.OnHoverChanged -= (view, isHovered) => OnCardHovered?.Invoke(this, isHovered);
    }

    public void SetHandHoverState(bool isHovered) {
        if (!_isInteractable) return;
        View.SetHoverState(isHovered);
    }

    public void SetInteractable(bool isEnabled) {
        _isInteractable = isEnabled;
        View.SetHoverState(false);
    }

    #region UI Info Update
    private void UpdateUIInfo() {
        CardDisplayData cardDisplayData = ConvertToDisplayData(Card);
        CardDisplayConfig cardDisplayConfig = CardDisplayConfig.ForHandCard();
        if (!(Card is CreatureCard card)) {
            cardDisplayConfig.showStats = false;
        }
        CardDisplayContext context = new(cardDisplayData, cardDisplayConfig);
        View.UpdateDisplay(context);
    }

    private CardDisplayData ConvertToDisplayData(Card card) {
        int attack = 0;
        int health = 0;
        if (card is CreatureCard creature) {
            attack = creature.Attack.Current;
            health = creature.Health.Current;
        }

        return new CardDisplayData {
            name = card.Data.Name,
            cost = card.Cost.Current,
            attack = attack,
            health = health,
            portrait = card.Data.Portait,
            background = card.Data.Background,
            rarity = RarityUtility.GetRarityColor(card.Data.Rarity)
        };
    }

    #endregion

    #region Movement API - основне для інших модулів

    /// <summary>
    /// Плавний рух до позиції (для руки, реорганізації)
    /// </summary>
    public async UniTask DoTweener(Tweener twenner) {
        await View.DoTweener(twenner);
    }

    /// <summary>
    /// Простий рух до позиції з поворотом за час
    /// </summary>
    public async UniTask DoSequence(Sequence sequence) {
        await View.DoSequence(sequence);
    }


    /// <summary>
    /// Почати фізичний рух (для драгу, таргетингу)
    /// </summary>
    public void DoPhysicsMovement(Vector3 initialPosition) {
        View?.DoPhysicsMovement(initialPosition);
    }

    /// <summary>
    /// Зупинити всі рухи
    /// </summary>
    public void StopMovement() {
        View?.StopMovement();
    }
    #endregion

    #region Render Order
    public void SetSortingOrder(int sortingOrder) {
        View.SetRenderOrder(sortingOrder);
    }

    public void ModifyRenderOrder(int value) {
        View.ModifyRenderOrder(value);
    }

    public void ResetRenderOrder() {
        View.ResetRenderOrder();
    }
    #endregion

    #region UnitPresenter API
    public override UnitModel GetModel() {
        return Card;
    }

    #endregion
}

