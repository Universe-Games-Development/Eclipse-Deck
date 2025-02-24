using System.Collections.Generic;

public struct AttackData {
    public Dictionary<Field, int> fieldDamageData;

    public void AddFieldsDamage(List<Field> fieldsToDamage, int damage) {
        foreach (var field in fieldsToDamage) {
            if (field != null) AddFieldDamage(field, damage);
        }
    }

    public void AddFieldDamage(Field fieldToDamage, int damage) {
        if (fieldDamageData == null) fieldDamageData = new Dictionary<Field, int>(1);
        if (fieldDamageData.ContainsKey(fieldToDamage)) {
            fieldDamageData[fieldToDamage] += damage;
        } else {
            fieldDamageData[fieldToDamage] = damage;
        }
    }
}
