using System;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour {

    [SerializeField] private GameObject worldSpaceCanvasPrefab; // Префаб World Space Canvas
    public Canvas WorldSpaceCanvas { get; private set; }

    public Action<ITipProvider> OnInfoItemEnter;

    private void Awake() {
        // Перевіряємо чи вже існує World Space Canvas
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        WorldSpaceCanvas = canvases.FirstOrDefault(canvas => canvas.renderMode == RenderMode.WorldSpace);

        if (WorldSpaceCanvas == null || WorldSpaceCanvas.renderMode != RenderMode.WorldSpace) {
            // Якщо немає, створюємо його
            GameObject canvasObject = Instantiate(worldSpaceCanvasPrefab);
            WorldSpaceCanvas = canvasObject.GetComponent<Canvas>();
        }
    }

    public void ShowTip(ITipProvider tipItem) {
        OnInfoItemEnter?.Invoke(tipItem);
    }

    /*
     public CreatureUI CreateCreatureUI(Card card) {
        // Создаем панель
        var panelObj = panelDistributer.CreateObject();

        // Привязываем панель к данным
        var creatureUI = panelObj.GetComponent<CreatureUI>();
        creatureUI.Initialize(panelDistributer, card);
        return creatureUI;
    }

    public CardUI CreateCardUI(Card card) {
        // Создаем панель
        var panelObj = cardDistributer.CreateObject();

        // Привязываем панель к данным
        var cardUI = panelObj.GetComponent<CardUI>();
        cardUI.Initialize(cardDistributer, card);
        return cardUI;
    }
     */
}
