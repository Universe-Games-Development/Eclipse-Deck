using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureCard", menuName = "TGE/Cards/CreatureCard")]
public class CreatureCardData : CardData {
    [Header("Creature Data")]
    public int MAX_CARD_ATTACK = 100;
    public int MAX_CARD_HEALTH = 100;

    public int Attack;
    public int Health;

    [Header("Operation Template")]
    [SerializeField] private OperationData _summonOperationTemplate;

    public void OnValidate() {
        SummonOperationData summonOperation = operationsData
            .Find(op => op is SummonOperationData) as SummonOperationData;

        if (summonOperation == null) {
            // Знаходимо шаблон за замовчуванням, якщо не вказано
            if (_summonOperationTemplate == null) {
                _summonOperationTemplate = Resources.Load<OperationData>("OperationTemplates/SummonOperationData");
            }

            if (_summonOperationTemplate == null) {
                Debug.LogWarning("Failed to create spawnOperation");
                return;
            }

            operationsData.Add(_summonOperationTemplate);
        }

        EditorUtility.SetDirty(this);
    }
}
