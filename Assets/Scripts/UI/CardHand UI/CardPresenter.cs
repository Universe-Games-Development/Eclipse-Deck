using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardPresenter : MonoBehaviour {
    public Card Card { get; private set; }
    public CardView View { get; private set; }

    public void Initialize(Card card, CardView cardView) {
        this.Card = card;
        View = cardView;

        CardUIInfo cardInfo = cardView.CardInfo;
        cardInfo.BatchUpdate( cardInfo => {
            cardInfo.UpdateCost(card.Cost.CurrentValue);
            cardInfo.UpdateName(card.Data.Name);
        });
    }

    private void UpdateCost(int value) {
        View.CardInfo.UpdateCost(value);
    }

    private void UpdateAttack(int value) {
        View.CardInfo.UpdateAttack(value);
    }

    private void UpdateHealth(int value) {
        View.CardInfo.UpdateHealth(value);
    }

    private void UpdateDescriptionContent(Card card) {

    }

    public void HandleRemoval() {
        View.PlayRemovalAnimation().Forget();
    }

    #region Movement API - основне для інших модулів

    /// <summary>
    /// Плавний рух до позиції (для руки, реорганізації)
    /// </summary>
    public void MoveTo(Vector3 position, Quaternion rotation, Vector3 scale, float duration = 1f, System.Action onComplete = null) {
        View?.MoveTo(position, rotation, scale, duration, onComplete);
    }

    /// <summary>
    /// Простий рух до позиції
    /// </summary>
    public void MoveTo(Vector3 position, float duration = 1f, System.Action onComplete = null) {
        MoveTo(position, transform.rotation, transform.localScale, duration, onComplete);
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
    public void StartPhysicsMovement(Vector3 initialPosition) {
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

    public void Reset() {
        View.Reset();
    }

    #endregion

}

