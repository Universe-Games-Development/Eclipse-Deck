using UnityEngine;

public abstract class CardAbilitySO : ScriptableObject {
    public string Name;
    public string Description;
    public Sprite Sprite;
    public CardState activationState;
    public EventType eventTrigger;

    public virtual bool ActivateAbility(GameContext gameContext) {
        Debug.Log("Base ability activation");
        return true;
    }

    // ����������� ������, ���� ��'��� ��������� � ��������
    private void OnValidate() {
        if (string.IsNullOrEmpty(Name)) {
            Name = GetType().Name;
        }
    }
}
