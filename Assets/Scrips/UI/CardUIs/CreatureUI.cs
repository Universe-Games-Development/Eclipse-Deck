using UnityEngine;

public class CreatureUI : CardRepresentative {
    public override void Initialize(Card card) {
        base.Initialize(card);
        UpdateAbilities(card.abilities);
    }

    public void PositionPanelInWorld(Transform uiPosition) {
        rectTransform.position = uiPosition.position;
        rectTransform.rotation = uiPosition.rotation;
    }
}
