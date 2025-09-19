using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ZoneDebug : MonoBehaviour {
    [SerializeField] ZonePresenter zonePresenter;
    [SerializeField] Button removeCraetureButton;
    private void Awake() {
        if (removeCraetureButton != null) {
            removeCraetureButton.onClick.AddListener(OnRemoveCreatureClicked);
        }
    }

    private void OnRemoveCreatureClicked() {
        Creature creature = zonePresenter.Zone.GetCreatures().FirstOrDefault();
        zonePresenter.Zone.RemoveCreature(creature);
    }
}
