using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureCard", menuName = "TGE/Cards/CreatureCard")]
public class CreatureCardData : CardData {
    [Header ("Creature Data")]
    public int MAX_CARD_ATTACK = 100;
    public int MAX_CARD_HEALTH = 100;

    public int Attack;
    public int Health;

    [Header("Operation Template")]
    [SerializeField] private SummonOperationData _summonOperationTemplate;

    public void OnValidate() 
    {
        // Знаходимо шаблон за замовчуванням, якщо не вказано
        if (_summonOperationTemplate == null)
        {
            _summonOperationTemplate = Resources.Load<SummonOperationData>("OperationTemplates/DefaultSummonOperation");
        }

        var summonOperation = operationsData
            .Find(op => op is SummonOperationData) as SummonOperationData;

        if (summonOperation == null) {
            operationsData.Add(_summonOperationTemplate);
        }
        
        EditorUtility.SetDirty(this);
    }
}
