using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CreatureUI : MonoBehaviour {
    [Header("UI Set Up")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private RectTransform abilitieFiller;
    [SerializeField] private Image rarityImage;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor;
    [SerializeField] private Color uncommonColor;
    [SerializeField] private Color rareColor;
    [SerializeField] private Color epicColor;
    [SerializeField] private Color legendaryColor;

    private Dictionary<Rarity, Color> rarityColors;

    [Header("References")]
    private RectTransform rectTransform;
    private Card card;
    private CreaturePanelDistributer panelDistributer;

    private List<Image> abilityImages = new List<Image>();

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();

        // Ініціалізація словника рідкостей
        rarityColors = new Dictionary<Rarity, Color> {
            { Rarity.Common, commonColor },
            { Rarity.Uncommon, uncommonColor },
            { Rarity.Rare, rareColor },
            { Rarity.Epic, epicColor },
            { Rarity.Legendary, legendaryColor }
        };
    }

    private void Update() {
        // Поворачиваем UI к основной камере
        if (Camera.main != null && rectTransform != null) {
            Vector3 directionToCamera = rectTransform.position - Camera.main.transform.position;
            directionToCamera.z = 0;
            rectTransform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    public void Initialize(CreaturePanelDistributer panelDistributer, BattleCreature creature, Field field) {
        this.card = creature.card;
        this.panelDistributer = panelDistributer;

        AttachmentToCard(card);

        PositionPanelInWorld(field.uiPoint);
    }

    private void AttachmentToCard(Card card) {
        if (card == null) return;

        card.Health.OnValueChanged += UpdateHealth;
        card.Health.OnDeath += OnCreatureDeath;
        card.Attack.OnValueChanged += UpdateAttack;

        UpdateName(card.Name);
        UpdateCost(card.Cost.CurrentValue, card.Cost.InitialValue);
        UpdateHealth(card.Health.CurrentValue, card.Health.InitialValue);
        UpdateAttack(card.Attack.CurrentValue, card.Attack.InitialValue);
        UpdateAbilities(card.abilities);
        UpdateRarityColor(card.Rarity);
    }

    private void UpdateRarityColor(Rarity rarity) {
        if (rarityColors.TryGetValue(rarity, out Color color)) {
            rarityImage.color = color;
        } else {
            Debug.LogWarning($"Color not found for rarity: {rarity}");
        }
    }

    private void UpdateName(string newName) {
        nameText.text = newName;
    }

    private void UpdateCost(int currentCost, int InitialValue) {
        costText.text = $"{currentCost}";
    }


    private void UpdateHealth(int currentHealth, int InitialValue) {
        healthText.text = $"{currentHealth}";
    }

    private void UpdateAttack(int currentAttack, int InitialValue) {
        attackText.text = $"{currentAttack}";
    }

    private void UpdateAbilities(List<CardAbility> abilities) {
        ClearAbilities();
        foreach (var ability in abilities) {
            if (ability.sprite != null) {
                Image abilityImage = CreateAbilityImage(ability.sprite);
                abilityImages.Add(abilityImage);
            }
        }
    }

    private Image CreateAbilityImage(Sprite sprite) {
        GameObject imageObj = new GameObject("AbilityImage", typeof(Image));
        imageObj.transform.SetParent(abilitieFiller, false);

        Image image = imageObj.GetComponent<Image>();
        image.sprite = sprite;
        return image;
    }

    private void ClearAbilities() {
        foreach (var image in abilityImages) {
            if (image != null) {
                Destroy(image.gameObject);
            }
        }
        abilityImages.Clear();
    }

    private void OnCreatureDeath() {
        Reset();
        panelDistributer.ReleasePanel(gameObject);
    }

    public void Reset() {
        if (card != null) {
            card.Health.OnValueChanged -= UpdateHealth;
            card.Health.OnDeath -= OnCreatureDeath;
        }

        ClearAbilities();
        card = null;
    }

    private void PositionPanelInWorld(Transform uiPosition) {
        rectTransform.position = uiPosition.position;
        rectTransform.rotation = uiPosition.rotation;
    }

    private void OnDestroy() {
        if (card != null) {
            card.Health.OnValueChanged -= UpdateHealth;
            card.Health.OnDeath -= OnCreatureDeath;
        }
        card = null;
    }
}
