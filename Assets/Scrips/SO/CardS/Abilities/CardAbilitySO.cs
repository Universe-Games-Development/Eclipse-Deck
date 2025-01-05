using UnityEngine;

public abstract class CardAbilitySO : ScriptableObject {
    public string abilityName;
    public string abilityDescription;
    public CardState activationState;
    public EventType eventTrigger;

    public virtual bool ActivateAbility(GameContext gameContext) {
        Debug.Log("Base ability activation");
        return true;
    }

    // ����������� ������, ���� ��'��� ��������� � ��������
    private void OnValidate() {
        if (string.IsNullOrEmpty(abilityName)) {
            abilityName = GetType().Name;
        }
    }
}
