using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardDescription : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RectTransform abilityFiller;
    private CardAbilityPool cardAbilityPool;
    List<CardAbilityUI> abilityUIs = new();

    public void SetDescripton(string description) {
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(true);
            descriptionText.text = description;
        }
    }

    public void UpdateAbilities() {
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(false);
        }
        //foreach (var ability in cardAbilities) {
        //    if (ability == null || ability.Data == null) continue;

        //    CardAbilityUI abilityUI = cardAbilityPool.Get();
        //    abilityUI.transform.SetParent(abilityFiller);

        //    abilityUI.FillAbilityUI(ability, true);
        //    abilityUIs.Add(abilityUI);
        //}
    }

    internal void SetAbilityPool(CardAbilityPool abilityPool) {
        throw new NotImplementedException();
    }
}