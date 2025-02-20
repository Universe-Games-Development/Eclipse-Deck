using UnityEngine;

// This class creates CardUI object and assign ghost layout to it so it knows own target position
// 2 object pools for cards and it`s layout ghosts
public class UICardFactory : MonoBehaviour {
    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform ghostLayoutParent;

    [SerializeField] private CardUI cardPrefab;
    [SerializeField] private CardLayoutGhost ghostPrefab;
    [SerializeField] private CardAbilityUI abilityPrefab;

    private CardUIPool cardPairPool;
    private CardGhostPool ghostPool;

    private void Awake() {
        InitializePools();
    }

    private void InitializePools() {
        ghostPool = new CardGhostPool(ghostPrefab, ghostLayoutParent);
        CardAbilityPool abilityUIPool = new CardAbilityPool(abilityPrefab, cardSpawnPoint);

        cardPairPool = new CardUIPool(cardPrefab, ghostPool, abilityUIPool, cardSpawnPoint); 
    }

    public CardUI CreateCardUI(Card card) {
        CardUI cardUI = cardPairPool.Get();
        cardUI.transform.position = cardSpawnPoint.position;
        cardUI.SetCardLogic(card);
        return cardUI;
    }

    public void ReleaseCardUI(CardUI cardUI) {
        cardUI.Reset();
        cardPairPool.Release(cardUI);
    }
}
