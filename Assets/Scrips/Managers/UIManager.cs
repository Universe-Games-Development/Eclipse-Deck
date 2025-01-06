using System;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour {
    [SerializeField] private CreaturePanelDistributer panelDistributer;
    [SerializeField] private GameObject worldSpaceCanvasPrefab; // Префаб World Space Canvas
    public Canvas WorldSpaceCanvas { get; private set; }

    public Action<ITipProvider> OnInfoItemEnter;
    public Action<ITipProvider> OnInfoItemExit;

    private void Awake() {
        // Перевіряємо чи вже існує World Space Canvas
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        WorldSpaceCanvas = canvases.FirstOrDefault(canvas => canvas.renderMode == RenderMode.WorldSpace);

        if (WorldSpaceCanvas == null || WorldSpaceCanvas.renderMode != RenderMode.WorldSpace) {
            // Якщо немає, створюємо його
            GameObject canvasObject = Instantiate(worldSpaceCanvasPrefab);
            WorldSpaceCanvas = canvasObject.GetComponent<Canvas>();
        }
        panelDistributer.Initialize(WorldSpaceCanvas);
    }

    public void HideTip(ITipProvider tipItem) {
        OnInfoItemExit.Invoke(tipItem);
    }

    public void ShowTip(ITipProvider tipItem) {
        OnInfoItemEnter?.Invoke(tipItem);
    }

    public void CreateCreatureUI(BattleCreature creature, Field field) {
        // Создаем панель
        var panelObj = panelDistributer.CreatePanel();

        // Привязываем панель к данным
        var creatureUI = panelObj.GetComponent<CreatureUI>();
        creatureUI.Initialize(panelDistributer, creature, field);
    }
}
