using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour {
    [Inject] public Player player;
    [Inject] GameboardController gameboard_c;

    [SerializeField] private CardHandUI handUI;
    [SerializeField] private int amountToDraw = 1;

    private RayService rayService;
    protected void Awake() {

        rayService = GetComponent<RayService>();
    }

    protected void Start() {
        handUI.Initialize(player.hand);

        for (int i = 0; i < amountToDraw; i++) {
            Card card = player.deck.DrawCard();
            if (card == null) {
                Debug.LogWarning("Drawn null card!");
                return;
            }
            player.hand.AddCard(card);
        }
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3? mouseWorldPosition = rayService.GetRayMousePosition();
            Field field = gameboard_c.GetFieldByWorldPosition(mouseWorldPosition);

            TrySummonCard(field);
        }
    }


    private void TrySummonCard(Field field) {
        if (field != null) {
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

            bool isSummoned = gameboard_c.SummonCreature(player, selectedCard, field);
            
            if (isSummoned) {
                player.hand.RemoveCard(selectedCard);
            }
        } else {
            handUI.DeselectCurrentCard();
        }
    }

}
