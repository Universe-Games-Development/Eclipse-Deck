using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class CardUI : CardRepresentative, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    [Header("Bottom Layer")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image characterImage;

    [Header("Middle Layer")]
    [SerializeField] private TextMeshProUGUI authorTMP;

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

    public override void Initialize(IObjectDistributer distributor, Card card) {
        base.Initialize(distributor, card);

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
        if (abilities == null) return;

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

    protected override void OnDestroy() {
        base.OnDestroy();
        OnCardClicked = null;
    }
}
