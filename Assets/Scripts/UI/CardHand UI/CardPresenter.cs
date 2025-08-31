using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class CardPresenter : UnitPresenter {
    public Action<CardPresenter> OnCardClicked;
    public Action<CardPresenter, bool> OnCardHovered;
    private const float defaultMoveDuration = 1f;

    

    public Card Card { get; private set; }
    public CardView View { get; private set; }

    [Header ("Debug")]
    [SerializeField] private bool DoTestRandomUpdate = true;
    [SerializeField] private int updateTimes = 15;
    [SerializeField] private float updateRate = 1.0f;

    private void OnDestroy() {
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

    #region UI Info Update
    private void UpdateUIInfo() {
        UpdateViewAppear();
        UpdateViewStats();
    }

    private void UpdateViewStats() {
        View.UpdateCost(Card.Cost.CurrentValue);

        var creatureCard = Card as CreatureCard;
        bool isCreatureCard = creatureCard != null;

        if (isCreatureCard) {
            View.UpdateAttack(creatureCard.Attack.CurrentValue);
            View.UpdateHealth(creatureCard.Health.CurrentValue);
        }

        View.ToggleCreatureStats(isCreatureCard);
    }

    private void UpdateViewAppear() {
        View.UpdateName(Card.Data.Name);
        View.UpdatePortait(Card.Data.Portait);
        View.UpdateBackground(Card.Data.BgImage);
        Color color = RarityUtility.GetRarityColor( Card.Data.Rarity);
        View.UpdateRarity(color);
    }

    #endregion

    #region Movement API - основне для інших модулів

    /// <summary>
    /// Плавний рух до позиції (для руки, реорганізації)
    /// </summary>
    public void MoveTo(Vector3 position, Quaternion rotation, Vector3 scale, float duration = defaultMoveDuration) {
        View?.MoveTo(position, rotation, scale, duration);
    }

    /// <summary>
    /// Простий рух до позиції з поворотом за час
    /// </summary>
    public void MoveTo(Vector3 position, Quaternion rotation, float duration = defaultMoveDuration) {
        MoveTo(position, rotation, transform.localScale, duration);
    }

    /// <summary>
    /// Миттєве переміщення
    /// </summary>
    public void SetPosition(Vector3 position, Quaternion rotation, Vector3 scale) {
        View?.SetPosition(position, rotation, scale);
    }

    /// <summary>
    /// Почати фізичний рух (для драгу, таргетингу)
    /// </summary>
    public void DoPhysicsMovement(Vector3 initialPosition) {
        View?.StartPhysicsMovement(initialPosition);
    }

    /// <summary>
    /// Оновлення цільової позиції в real-time
    /// </summary>
    public void UpdateTargetPosition(Vector3 position) {
        View?.UpdateTargetPosition(position);
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

    #region BoardUnit API
    public override UnitModel GetInfo() {
        return Card;
    }

    public override BoardPlayer GetPlayer() {
        return Card.Owner;
    }
    #endregion


    public void HandleRemoval() {
        //Debug.Log($"Card {Card.Data.Name} is being removed from hand and destroyed.");
    }

    public async UniTask LaunchTestUpdate() {
        for (int i = 0; i < updateTimes; i++) {
            int randomHealth = UnityEngine.Random.Range(1, 100);  
            View.UpdateHealth(randomHealth);
            int randomAttack = UnityEngine.Random.Range(1, 100);
            View.UpdateAttack(randomAttack);
            Debug.Log($"Iteration {i}, {randomAttack}/{randomHealth}");
            await UniTask.Delay(TimeSpan.FromSeconds(updateRate));
        }
    }

    public void SetHandHoverState(bool isHovered) {
        View.SetHoverState(isHovered);
    }

    internal void SetPool(Card3DPool cardPool) {
        throw new NotImplementedException();
    }
}

