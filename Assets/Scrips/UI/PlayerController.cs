using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour {
    [Inject] public Player player;
    [Inject] GameboardController gameboard_c;

    [SerializeField] private CardHandUI handUI;
    [SerializeField] private RayService rayService;

    protected void Start() {
        handUI.Initialize(player.hand);
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
            Card selectedCard = player.hand.SelectedCard;
            if (selectedCard == null) {
                Debug.Log("No card selected to summon.");
                return;
            }

            bool isSummoned = gameboard_c.SummonCreature(player, selectedCard, field);
            
            if (isSummoned) {
                player.hand.RemoveCard(selectedCard);
            }
        } else {
            player.hand.DeselectCurrentCard();
        }
    }

}
