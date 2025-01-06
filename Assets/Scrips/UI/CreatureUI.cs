using UnityEngine;

public class CreatureUI : MonoBehaviour {
    private Card card;
    private CreaturePanelDistributer panelDistributer;

    public void Initialize(CreaturePanelDistributer panelDistributer, BattleCreature creature) {
        this.card = creature.card;
        this.panelDistributer = panelDistributer;

        // ϳ��������� �� ��䳿 ������'� � �����
        card.Health.OnValueChanged += UpdateHealth;
        card.Health.OnDeath += OnCreatureDeath;

        // ����������� UI
        UpdateHealth(card.Health.CurrentValue, card.Health.MaxValue);
    }


    private void UpdateHealth(int currentHealth, int maxHealth) {
        // ����� ��������� ���������� ������'�
        Debug.Log($"Updating health: {currentHealth}/{maxHealth}");
    }

    private void OnCreatureDeath() {
        // ³��������� �� ����, ������� ���� � ��������� � ���
        Reset();
        panelDistributer.ReleasePanel(gameObject);
    }

    public void Reset() {
        // ³��������� �� ���� � ������� ����
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
