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
    private const string pathToSummonOperation = "Operations/Summon";

    protected override void Validate() {
        // Спочатку викликаємо батьківську валідацію
        base.Validate();

        // Додаємо специфічну для CreatureCardData логіку
        SummonOperationData summonOperation = operationsData
            .Find(op => op is SummonOperationData) as SummonOperationData;

        if (summonOperation == null) {
            if (_summonOperationTemplate == null) {
                _summonOperationTemplate = Resources.Load<SummonOperationData>(pathToSummonOperation);
            }

            if (_summonOperationTemplate != null) {
                operationsData.Add(_summonOperationTemplate);
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }
    }
}