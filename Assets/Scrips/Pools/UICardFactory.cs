﻿using UnityEngine;

// This class creates CardUI object and assign ghost layout to it so it knows own target position
// 2 object pools for cards and it`s layout ghosts
public class UICardFactory : MonoBehaviour {
    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform ghostLayoutParent;

    [SerializeField] private CardUI cardPrefab;
    [SerializeField] private CardLayoutGhost ghostPrefab;
    [SerializeField] private CardAbilityUI abilityPrefab;

    private CardPool cardPool;
    private CardGhostPool ghostPool;
    public CardAbilityPool AbilityUIPool { get; private set; }

    private void Awake() {
        InitializePools();
    }

    private void InitializePools() {
        ghostPool = new CardGhostPool(ghostPrefab, ghostLayoutParent);
        AbilityUIPool = new CardAbilityPool(abilityPrefab, cardSpawnPoint);

        cardPool = new CardPool(cardPrefab, ghostPool, cardSpawnPoint); 
    }

    public CardUI CreateCardUI(Card card) {
        CardUI cardUI = cardPool.Get();
        cardUI.transform.position = cardSpawnPoint.position;
        cardUI.SetCardLogic(card);
        cardUI.SetAbilityPool(AbilityUIPool); // Передаємо посилання на CardFactory

        return cardUI;
    }

    public void ReleaseCardUI(CardUI cardUI) {
        cardUI.Reset();
        cardPool.Release(cardUI);
    }
}
