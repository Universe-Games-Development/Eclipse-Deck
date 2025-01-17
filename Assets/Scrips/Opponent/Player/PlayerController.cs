using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour {
    [Inject] public Player player;

    [SerializeField] private CardHandUI handUI;
    
    private RayService rayService;
    protected void Awake() {

        rayService = GetComponent<RayService>();
    }

    protected void Start() {
        handUI.Initialize(player.hand);

        for (int i = 0; i < 3; i++) {
            player.hand.AddCard(player.deck.DrawCard());
        }
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            TrySummonCard();
        }
    }

    private void TrySummonCard() {
        GameObject gameObject = rayService.GetRayObject();
        if (gameObject && gameObject.TryGetComponent(out FieldVisual field)) {
            CardUI selectedUICard = handUI.SelectedCard;
            if (selectedUICard == null) {
                Debug.Log("No card selected to summon.");
                return;
            }

            Card selectedCard = player.hand.GetCardByID(selectedUICard.Id);
            if (selectedCard == null) {
                Debug.Log("Selected card not found in hand.");
                return;
            }

            player.hand.RemoveCard(selectedCard);
            Debug.Log($"Card {selectedCard.data.Name} summoned to field!");
        } else {
            handUI.DeselectCurrentCard();
        }
    }

}
