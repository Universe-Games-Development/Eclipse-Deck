using UnityEngine;
using UnityEngine.UI;

public class HandDebug : MonoBehaviour
{
    [SerializeField] CardHandView hand;

    [SerializeField] Button addCardButton;
    [SerializeField] int initialCards = 0;

    private void Start() {
        addCardButton.onClick.AddListener(() => {
            AddCard();
        });

        for (int i = 0; i < initialCards; i++) {
            AddCard();
        }
    }

    private void AddCard() {
        hand.CreateCardView();
    }
}
