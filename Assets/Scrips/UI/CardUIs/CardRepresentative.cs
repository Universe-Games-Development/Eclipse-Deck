using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class CardRepresentative : MonoBehaviour {
    [SerializeField] public string Id;

    [Header("UI")]
    [SerializeField] protected TextMeshProUGUI nameText;

    [Header("Params")]
    [SerializeField] protected TextMeshProUGUI healthText;
    [SerializeField] protected TextMeshProUGUI attackText;

    protected Card card;

    [SerializeField] protected ObjectDistributer abilityUIDisctibuter;
    protected List<AbilityUI> abilityUIs = new List<AbilityUI>();
    protected RectTransform rectTransform;

    [Header("UI Set Up")]
    [SerializeField] private RectTransform abilitieFiller;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        // Поворачиваем UI к основной камере
        if (Camera.main != null && rectTransform != null) {
            Vector3 directionToCamera = rectTransform.position - Camera.main.transform.position;
            directionToCamera.z = 0;
            rectTransform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    public virtual void Initialize(Card card) {
        this.card = card;
        Id = card.Id;

        AttachmentToCard(card);

        // Безопасное обновление UI
        UpdateName(card?.data?.name);
        UpdateHealth(card?.Health?.CurrentValue ?? 0, card?.Health?.CurrentValue ?? 0);
        UpdateAttack(card?.Attack?.CurrentValue ?? 0, card?.Attack?.CurrentValue ?? 0);
    }

    protected virtual void AttachmentToCard(Card card) {
        if (card == null) return;

        card.Health.OnValueChanged += UpdateHealth;
        card.Attack.OnValueChanged += UpdateAttack;
    }

    protected virtual void UpdateName(string newName) {
        if (nameText != null && !string.IsNullOrEmpty(newName)) {
            nameText.text = newName;
        } else {
            Debug.LogWarning("Attempted to set name to an empty or null value.");
        }
    }

    protected virtual void UpdateHealth(int currentHealth, int initialHealth) {
        if (healthText != null) {
            healthText.text = currentHealth > 0 ? $"{currentHealth}" : "0";
        }
    }

    protected virtual void UpdateAttack(int currentAttack, int initialAttack) {
        if (attackText != null) {
            attackText.text = currentAttack >= 0 ? $"{currentAttack}" : "0";
        }
    }

    // Creature UI will use this
    // CardUI will override this
    protected virtual void UpdateAbilities(List<CardAbility> abilities) {
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
                abilityUI.CreateUISets(ability, false);
                abilityUIs.Add(abilityUI);

                // Робимо об'єкт дочірнім елементом
                newAbilityObj.transform.SetParent(abilitieFiller, false);
            }
        }
    }

    protected virtual void ClearAbilities() {
        foreach (var abilityUI in abilityUIs) {
            if (abilityUI != null) {
                abilityUI.ResetUI();
            }
        }
        abilityUIs.Clear();
    }

    public virtual void Reset() {
        if (card != null) {
            card.Health.OnValueChanged -= UpdateHealth;
            card.Attack.OnValueChanged -= UpdateAttack;
        }

        ClearAbilities();
        card = null;
    }

    protected virtual void OnDestroy() {
        Reset();
    }
}
