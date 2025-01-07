using UnityEngine;

public class CreatureUI : CardRepresentative {
    public override void Initialize(IObjectDistributer distributer, Card card) {
        base.Initialize(distributer, card);
        UpdateAbilities(card.abilities);
    }

    public void PositionPanelInWorld(Transform uiPosition) {
        rectTransform.position = uiPosition.position;
        rectTransform.rotation = uiPosition.rotation;
    }
}
