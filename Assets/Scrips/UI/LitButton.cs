using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
    public Color normalColor = Color.white;
    public Color highlightedColor = Color.grey;
    public Color pressedColor = Color.black;

    private Material materialInstance;
    private Button button;

    [SerializeField] private Texture buttonSprite;

    private bool isHighlighted;

    void Start() {
        button = GetComponent<Button>();
        var image = GetComponent<Image>();

        if (image != null) {
            // Створення унікального інстансу матеріалу
            materialInstance = Instantiate(image.material);
            image.material = materialInstance;
            materialInstance.mainTexture = buttonSprite;
        }

        if (button != null) {
            button.onClick.AddListener(OnButtonPressed);
        }

        UpdateColor(normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (button != null && button.IsInteractable()) {
            isHighlighted = true;
            UpdateColor(highlightedColor);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHighlighted = false;
        UpdateColor(normalColor);
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (button != null && button.IsInteractable()) {
            UpdateColor(pressedColor);
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (button != null && button.IsInteractable()) {
            UpdateColor(isHighlighted ? highlightedColor : normalColor);
        }
    }

    private void OnButtonPressed() {
        // Додаткова логіка при натисканні, якщо потрібно
    }

    private void UpdateColor(Color color) {
        if (materialInstance != null) {
            materialInstance.SetColor("_BaseColor", color); // Використовуйте "_BaseColor" для URP Lit
        }
    }
}
