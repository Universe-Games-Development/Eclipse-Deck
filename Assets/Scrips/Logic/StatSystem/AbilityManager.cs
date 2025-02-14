using System;
using System.Collections.Generic;
using System.Linq;

public class AbilityManager {
    private List<Ability> abilities = new();
    private IAbilityOwner abilityOwner;
    private GameEventBus eventBus;

    public AbilityManager(IAbilityOwner abilityOwner, GameEventBus eventBus) { 
        this.abilityOwner = abilityOwner;
        this.eventBus = eventBus;
    }

    public void InitializeAbilities(List<AbilitySO> abilitiesData) {
        foreach (var abilityData in abilitiesData) {
            Ability ability = abilityData.GenerateAbility(abilityOwner, eventBus);
            abilities.Add(ability);
        }
        // 
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
