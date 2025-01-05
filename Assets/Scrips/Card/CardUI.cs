using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private string id;
    public string Id { get { return id; } private set { } }

    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardDescriptionText;
    [SerializeField] private TextMeshProUGUI cost;

    private Animator animator;
    private int originalSiblingIndex;

    public Card Card { get; private set; } // Геттер для доступу до карти
    public event System.Action<CardUI> OnCardClicked;

    [SerializeField] private bool isSelected;

    private void Awake() {
        animator = GetComponent<Animator>();
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    public void Initialize(Card card) {
        if (card == null) {
            Debug.LogError("Card is null during initialization!");
            return;
        }

        id = card.Id;
        Card = card;
        cardNameText.text = card.Name;
        cardDescriptionText.text = card.Description;
        cost.text = card.Cost.ToString();
        cardImage.sprite = card.MainImage;
    }

    public void OnPointerClick(PointerEventData eventData) {
        OnCardClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        transform.SetSiblingIndex(transform.parent.childCount - 1);
        if (animator != null) {
            animator.SetBool("Lift", true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        transform.SetSiblingIndex(originalSiblingIndex);
        if (animator != null) {
            animator.SetBool("Lift", false);
        }
    }

    public void SelectCard() {
        isSelected = true;
        if (animator != null) {
            animator.SetBool("Selected", isSelected);
        }
        cardImage.color = Color.green; // Візуальна індикація
    }

    public void DeselectCard() {
        isSelected = false;
        if (animator != null) {
            animator.SetBool("Selected", isSelected);
        }
        
        cardImage.color = Color.white; // Відновлення стандартного кольору
    }

    private void OnDestroy() {
        OnCardClicked = null;
    }
}
