using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class ZonePresenter : UnitPresenter {
    public Zone Zone;
    public Zone3DView View;
    [SerializeField] public BoardPlayer Owner;
    [Inject] IUnitPresenterRegistry unitPresenterRegistry;

    private readonly List<CreaturePresenter> creaturesInZone = new();

    private void Start() {
        Zone = new Zone();
        Zone.ChangeOwner(Owner);
        Zone.OnCreaturePlaced += HandleCreaturePlacement;
        Zone.OnCreatureRemoved += HandleCreatureRemove;
    }

    public void Initialize(Zone3DView zone3DView, Zone zone) {
        View = zone3DView;
        Zone = zone;
    }

    private void HandleCreaturePlacement(Creature creature) {
        DebugLog("Creature spawned event received");

        CreaturePresenter creaturePresenter = unitPresenterRegistry.GetPresenter<CreaturePresenter>(creature);
        creaturesInZone.Add(creaturePresenter);

        // Отримуємо позиції від View і керуємо переміщенням
        var positions = View.GetCreaturePoints(creaturesInZone.Count);
        RearrangeCreatures(positions);

        View.UpdateSummonedCount(Zone.GetCreaturesCount());
    }

    private void HandleCreatureRemove(Creature creature) {
        DebugLog("Creature removed event received");

        // Знаходимо і видаляємо creaturePresenter
        var creatureToRemove = creaturesInZone.FirstOrDefault(c => c.Creature == creature);
        if (creatureToRemove != null) {
            creaturesInZone.Remove(creatureToRemove);
            View.UpdateSummonedCount(creaturesInZone.Count);

            // Перерозподіляємо решту істот
            var positions = View.GetCreaturePoints(creaturesInZone.Count);
            RearrangeCreatures(positions);
        }
    }

    public bool PlaceCreature(Creature creature) {
        DebugLog($"Placing creature: {creature}");
        Zone.PlaceCreature(creature);
        return true;
    }

    [SerializeField] private float cardsOrganizeDuration = 0.5f;
    // VISUAL TASK
    private void RearrangeCreatures(List<TransformPoint> positions) {
        for (int i = 0; i < creaturesInZone.Count; i++) {
            if (i < positions.Count) {
                CreaturePresenter creaturePresenter = creaturesInZone[i];
                Transform cardTransform = creaturePresenter.transform;
                TransformPoint point = positions[i];

                Tweener moveTween = cardTransform.DOMove(point.position, cardsOrganizeDuration)
                                    .SetEase(Ease.OutQuad)
                                    .SetLink(cardTransform.gameObject);

                creaturePresenter.View.DoTweener(moveTween);
            }
        }
    }

    public override void Highlight(bool enable) {
        DebugLog($"Setting highlight to: {enable}");
        View.Highlight(enable);
    }

    #region UnitInfo API
    public override UnitModel GetModel() {
        return Zone;
    }

    public override BoardPlayer GetPlayer() {
        DebugLog($"Getting owner: {Owner?.name}");
        return Owner;
    }
    #endregion
}
