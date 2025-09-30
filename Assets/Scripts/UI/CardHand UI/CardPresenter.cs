using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CardPresenter : InteractablePresenter {
    public Card Card { get; private set; }
    public CardView CardView { get; private set; }

    public CardPresenter (Card card, CardView cardView) : base(card, cardView) {
        this.Card = card;
        CardView = cardView;

        UpdateUIInfo();
    }

    public void SetHandHoverState(bool isHovered) {
        CardView.SetHoverState(isHovered);
    }


    #region UI Info Update
    private void UpdateUIInfo() {
        CardDisplayData cardDisplayData = ConvertToDisplayData(Card);
        CardDisplayConfig cardDisplayConfig = CardDisplayConfig.ForHandCard();
        if (!(Card is CreatureCard card)) {
            cardDisplayConfig.showStats = false;
        }
        CardDisplayContext context = new(cardDisplayData, cardDisplayConfig);
        CardView.UpdateDisplay(context);
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
        await CardView.DoTweener(twenner);
    }

    /// <summary>
    /// Простий рух до позиції з поворотом за час
    /// </summary>
    public async UniTask DoSequence(Sequence sequence) {
        await CardView.DoSequence(sequence);
    }

    /// <summary>
    /// Почати фізичний рух (для драгу, таргетингу)
    /// </summary>
    public void DoPhysicsMovement(Vector3 initialPosition) {
        CardView?.DoPhysicsMovement(initialPosition);
    }

    /// <summary>
    /// Зупинити всі рухи
    /// </summary>
    public void StopMovement() {
        CardView?.StopMovement();
    }
    #endregion

    #region Render Order
    public void SetSortingOrder(int sortingOrder) {
        CardView.SetRenderOrder(sortingOrder);
    }

    public void ModifyRenderOrder(int value) {
        CardView.ModifyRenderOrder(value);
    }

    public void ResetRenderOrder() {
        CardView.ResetRenderOrder();
    }
    #endregion


    public void ToggleTiltMovement(bool v) {
        CardView.ToggleTiling(v);
    }
}

