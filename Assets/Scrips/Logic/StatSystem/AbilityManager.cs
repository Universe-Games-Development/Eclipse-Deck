using System;
using System.Collections.Generic;
using System.Linq;

public class AbilityManager {
    private List<Ability> abilities = new();

    public void InitializeAbilities(IAbilityOwner abilityOwner, List<AbilitySO> abilitiesData) {
        foreach (var abilityData in abilitiesData) {
            Ability ability = abilityData.GenerateAbility(abilityOwner);
            abilities.Add(ability);
        }
    }

    public List<Ability> GetNonActiveAbilities() {
        return abilities.Where(ability => !ability.IsActive).ToList();
    }

    public List<Ability> GetAbilities() {
        return abilities;
    }

    public List<Ability> GetAbilities(Func<Ability, bool> predicate) {
        return abilities.Where(predicate).ToList();
    }
}
