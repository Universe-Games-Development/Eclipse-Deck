using UnityEngine;

public class Player : Opponent {
    [SerializeField] private CardHandUI handUI;

    private RayService rayService;
    private void Awake() {
        base.Awake();
        rayService = GetComponent<RayService>();
    }

    protected override void Start() {
        base.Start();
        handUI.Initialize(hand);



        hand.AddCard(deck.DrawCard());
        hand.AddCard(deck.DrawCard());
        hand.AddCard(deck.DrawCard());
        hand.AddCard(deck.DrawCard());
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            TrySummonCard();
        }
    }

    private void TrySummonCard() {
        GameObject gameObject = rayService.GetRayObject();
        if (gameObject && gameObject.TryGetComponent(out Field field)) {
            CardUI selectedUICard = handUI.SelectedCard;
            if (selectedUICard == null) {
                Debug.Log("No card selected to summon.");
                return;
            }

            Card selectedCard = hand.GetCardByID(selectedUICard.Id);
            if (selectedCard == null) {
                Debug.Log("Selected card not found in hand.");
                return;
            }

            bool isPlayed = gameBoard.SummonCreature(this, selectedCard, field);
            if (isPlayed) {
                hand.RemoveCard(selectedCard);
                Debug.Log($"Card {selectedCard.data.Name} summoned to field!");
            }
        } else {
            handUI.DeselectCurrentCard();
        }
    }

}
