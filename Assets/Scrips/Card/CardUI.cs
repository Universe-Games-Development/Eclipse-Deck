using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : CardRepresentative, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    [Header("Bottom Layer")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image characterImage;

    [Header("Middle Layer")]
    [SerializeField] private TextMeshProUGUI authorTMP;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Top Layer")]
    [SerializeField] private Image rarity;
    [SerializeField] private TextMeshProUGUI costTMP;

    private Animator animator;
    private int originalSiblingIndex;
    public event System.Action<CardUI> OnCardClicked;

    private bool isSelected;

    private void Awake() {
        animator = GetComponent<Animator>();
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    public override void Initialize(Card card) {
        base.Initialize(card);

        if (card == null) {
            Debug.LogError("Card is null during initialization!");
            return;
        }

        // Top Layer

        rarity.color = card.data.GetRarityColor();
        authorTMP.text = card.data.AuthorName;

        characterImage.sprite = card.data.characterSprite;
        UpdateAbilities(card.abilities);

        UpdateCost(card?.Cost?.CurrentValue ?? 0, card?.Cost?.CurrentValue ?? 0);
        card.Cost.OnValueChanged += UpdateCost;
    }


    protected override void UpdateAbilities(List<CardAbility> abilities) {
        if (abilities == null || abilities.Count == 0) {
            EnableCardDescription();
            return;
        }

        // Якщо є здібності, приховуємо опис
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(false);
        }

        ClearAbilities();
        foreach (var ability in abilities) {
            if (ability != null && ability.data != null) {
                // Створення нової AbilityUI через дистриб'ютор
                GameObject newAbilityObj = abilityUIDisctibuter.CreateObject();
                if (newAbilityObj == null) continue;

                AbilityUI abilityUI = newAbilityObj.GetComponent<AbilityUI>();
                if (abilityUI == null) {
                    Debug.LogWarning("Created object does not have an AbilityUI component.");
                    Destroy(newAbilityObj);
                    continue;
                }

                // Налаштування UI об'єкта здібності
                abilityUI.CreateUISets(ability, true);
                abilityUIs.Add(abilityUI);
            }
        }
    }

    private void EnableCardDescription() {
        // Якщо немає здібностей, показуємо опис карти
        if (descriptionText != null && card != null) {
            descriptionText.text = card.data.description;
            descriptionText.gameObject.SetActive(true);  // Активуємо descriptionText
        }
    }

    protected void UpdateCost(int currentCost, int initialCost) {
        if (costTMP != null) {
            costTMP.text = currentCost >= 0 ? $"{initialCost}" : "0";
        }
    }

    #region User Interaction
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
        characterImage.color = Color.green;
    }

    public void DeselectCard() {
        isSelected = false;
        if (animator != null) {
            animator.SetBool("Selected", isSelected);
        }
        characterImage.color = Color.white;
    }
    #endregion

    public override void Reset() {
        base.Reset();
        if (card != null) {
            card.Cost.OnValueChanged -= UpdateCost;
        }

        // Очистка UI полів
        if (descriptionText != null) {
            descriptionText.text = string.Empty;
            descriptionText.gameObject.SetActive(false);
        }

        if (authorTMP != null) {
            authorTMP.text = string.Empty;
        }

        if (costTMP != null) {
            costTMP.text = "0";
        }

        if (characterImage != null) {
            characterImage.sprite = null;
            characterImage.color = Color.white;
        }

        if (rarity != null) {
            rarity.color = Color.white;
        }

        ClearAbilities();
        card = null;
        isSelected = false;
        if (animator != null) {
            animator.SetBool("Selected", false);
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();



        OnCardClicked = null;
    }
}
