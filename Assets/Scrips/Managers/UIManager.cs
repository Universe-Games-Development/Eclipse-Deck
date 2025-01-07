using System;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour {

    [SerializeField] private GameObject worldSpaceCanvasPrefab; // ������ World Space Canvas
    public Canvas WorldSpaceCanvas { get; private set; }

    public Action<ITipProvider> OnInfoItemEnter;

    private void Awake() {
        // ���������� �� ��� ���� World Space Canvas
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        WorldSpaceCanvas = canvases.FirstOrDefault(canvas => canvas.renderMode == RenderMode.WorldSpace);

        if (WorldSpaceCanvas == null || WorldSpaceCanvas.renderMode != RenderMode.WorldSpace) {
            // ���� ����, ��������� ����
            GameObject canvasObject = Instantiate(worldSpaceCanvasPrefab);
            WorldSpaceCanvas = canvasObject.GetComponent<Canvas>();
        }
    }

    public void ShowTip(ITipProvider tipItem) {
        OnInfoItemEnter?.Invoke(tipItem);
    }

    /*
     public CreatureUI CreateCreatureUI(Card card) {
        // ������� ������
        var panelObj = panelDistributer.CreateObject();

        // ����������� ������ � ������
        var creatureUI = panelObj.GetComponent<CreatureUI>();
        creatureUI.Initialize(panelDistributer, card);
        return creatureUI;
    }

    public CardUI CreateCardUI(Card card) {
        // ������� ������
        var panelObj = cardDistributer.CreateObject();

        // ����������� ������ � ������
        var cardUI = panelObj.GetComponent<CardUI>();
        cardUI.Initialize(cardDistributer, card);
        return cardUI;
    }
     */
}
