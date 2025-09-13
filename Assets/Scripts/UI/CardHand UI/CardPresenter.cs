using Cysharp.Threading.Tasks;
using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardPresenter : UnitPresenter {
    public Action<CardPresenter> OnCardClicked;
    public Action<CardPresenter, bool> OnCardHovered;
    public Card Card { get; private set; }
    public CardView View { get; private set; }

    [Header ("Debug")]
    [SerializeField] private bool DoTestRandomUpdate = true;
    [SerializeField] private int updateTimes = 15;
    [SerializeField] private float updateRate = 1.0f;
    [SerializeField] private bool _isInteractable = true;

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

    public List<OperationData> GetOperationDatas() {
        var operationDatas = new List<OperationData>(Card?.GetOperationData());

        // Додаємо операцію спавну для істот
        if (Card is CreatureCard creatureCard) {
            var spawnOp = CreateSpawnOperationData(creatureCard);
            operationDatas.Add(spawnOp);
        }

        return operationDatas;
    }

    private SpawnCreatureOperationData CreateSpawnOperationData(CreatureCard creatureCard) {
        var spawnOp = ScriptableObject.CreateInstance<SpawnCreatureOperationData>();
        spawnOp.creatureCard = creatureCard;
        spawnOp.presenter = this;
        spawnOp.spawnPosition = View.transform.position;
        return spawnOp;
    }


    #region UI Info Update
    private void UpdateUIInfo() {
        UpdateViewAppear();
        UpdateViewStats();
    }

    private void UpdateViewStats() {
        View.UpdateCost(Card.Cost.Current);

        var creatureCard = Card as CreatureCard;
        bool isCreatureCard = creatureCard != null;

        if (isCreatureCard) {
            View.UpdateAttack(creatureCard.Attack.Current);
            View.UpdateHealth(creatureCard.Health.Current);
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
    public void DoTweener(Tweener twenner) {
        View.DoTweener(twenner);
    }

    /// <summary>
    /// Простий рух до позиції з поворотом за час
    /// </summary>
    public void DoSequence(Sequence sequence) {
        View.DoSequence(sequence);
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
    public override UnitModel GetInfo() {
        return Card;
    }

    public override BoardPlayer GetPlayer() {
        return Card?.Owner;
    }
    #endregion
}

