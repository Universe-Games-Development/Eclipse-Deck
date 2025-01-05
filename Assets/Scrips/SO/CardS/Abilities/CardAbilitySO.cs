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

    // Викликається щоразу, коли об'єкт змінюється в редакторі
    private void OnValidate() {
        if (string.IsNullOrEmpty(abilityName)) {
            abilityName = GetType().Name;
        }
    }
}
