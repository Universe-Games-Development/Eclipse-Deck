using UnityEngine;
using UnityEngine.UI;

public class HandDebug : MonoBehaviour
{
    [SerializeField] CardHandView hand;

    [SerializeField] Button addCardButton;

    private void Awake() {
        addCardButton.onClick.AddListener(() => {
            hand.CreateCardView("Card" + Random.Range(0, 100));
        });
    }
}
