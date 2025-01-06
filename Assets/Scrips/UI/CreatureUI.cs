using UnityEngine;

public class CreatureUI : MonoBehaviour {
    private Card card;
    private CreaturePanelDistributer panelDistributer;

    public void Initialize(CreaturePanelDistributer panelDistributer, BattleCreature creature) {
        this.card = creature.card;
        this.panelDistributer = panelDistributer;

        // Підписуємося на події здоров'я і смерті
        card.Health.OnValueChanged += UpdateHealth;
        card.Health.OnDeath += OnCreatureDeath;

        // Ініціалізація UI
        UpdateHealth(card.Health.CurrentValue, card.Health.MaxValue);
    }


    private void UpdateHealth(int currentHealth, int maxHealth) {
        // Логіка оновлення інтерфейсу здоров'я
        Debug.Log($"Updating health: {currentHealth}/{maxHealth}");
    }

    private void OnCreatureDeath() {
        // Відписуємося від подій, скидаємо стан і повертаємо в пул
        Reset();
        panelDistributer.ReleasePanel(gameObject);
    }

    public void Reset() {
        // Відписуємося від подій і скидаємо стан
        if (card != null) {
            card.Health.OnValueChanged -= UpdateHealth;
            card.Health.OnDeath -= OnCreatureDeath;
        }

        card = null;
    }

    private void OnDestroy() {
        if (card != null) {
            card.Health.OnValueChanged -= UpdateHealth;
            card.Health.OnDeath -= OnCreatureDeath;
        }
        card = null;
    }
}
