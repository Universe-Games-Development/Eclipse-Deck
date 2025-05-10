using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ³������ �� ��������� UI-���� �� ������� ��� 3D-����
/// </summary>
public class CardTextureRenderer : MonoBehaviour {
    [SerializeField] private RenderingRoom staticRenderingRoom;
    [SerializeField] private RenderingRoom dynamicRenderingRoom;

    private Dictionary<Card3DView, CardUIView> cardMappings = new Dictionary<Card3DView, CardUIView>();
    private HashSet<CardUIView> dynamicCards = new HashSet<CardUIView>();

    private void LateUpdate() {
        // ��������� ������� ����� � ������� ����
        foreach (var cardUI in dynamicCards) {
            UpdateCardTexture(cardUI);
        }
    }

    /// <summary>
    /// ������ 3D-����� �� ������� ��� �� UI-�������������
    /// </summary>
    public void Register3DCard(Card3DView card3D, bool isDynamic = false) {
        // ��������� UI-����� � �������� ����� ����������
        CardUIView cardUIView = isDynamic
            ? dynamicRenderingRoom.CreateCardUI()
            : staticRenderingRoom.CreateCardUI();

        // ���������� 3D-����� � UI-��������������
        card3D.Initialize(cardUIView);

        // �������� ��'���� �� 3D �� UI �������
        cardMappings[card3D] = cardUIView;

        if (isDynamic) {
            dynamicCards.Add(cardUIView);
        }

        // ������ ������ ��������
        UpdateCardTexture(cardUIView);

        // ϳ��������� �� ���� � UI-����
        cardUIView.OnChanged += () => OnCardUIViewChanged(cardUIView);
    }

    /// <summary>
    /// ����� ����� ����� �� ��������� � ���������
    /// </summary>
    public void SetCardDynamic(Card3DView card3D, bool isDynamic) {
        if (!cardMappings.TryGetValue(card3D, out CardUIView cardUI))
            return;

        if (isDynamic && !dynamicCards.Contains(cardUI)) {
            // ���������� ����� � �������� � �������� ������
            staticRenderingRoom.RemoveCardUI(cardUI);
            dynamicRenderingRoom.AddCardUI(cardUI);
            dynamicCards.Add(cardUI);
        } else if (!isDynamic && dynamicCards.Contains(cardUI)) {
            // ���������� ����� � �������� � �������� ������
            dynamicRenderingRoom.RemoveCardUI(cardUI);
            staticRenderingRoom.AddCardUI(cardUI);
            dynamicCards.Remove(cardUI);

            // Գ������� ������ ��� �������� �����
            UpdateCardTexture(cardUI);
        }
    }

    /// <summary>
    /// �������� ���� � UI-���� (������ � ������ OnCardChanged)
    /// </summary>
    private void OnCardUIViewChanged(CardUIView cardUI) {
        // ���� ����� �������� - ��������� �� �������� �������
        if (!dynamicCards.Contains(cardUI)) {
            UpdateCardTexture(cardUI);
        }
        // ��� ��������� ���� ��������� ���������� � LateUpdate
    }

    /// <summary>
    /// ������� �������� ��� ��������� �����
    /// </summary>
    private void UpdateCardTexture(CardUIView cardUI) {
        RenderingRoom room = dynamicCards.Contains(cardUI) ? dynamicRenderingRoom : staticRenderingRoom;
        Texture2D texture = room.RenderCardTexture(cardUI);

        // ��������� �� 3D-�����, �� �������������� ��� UI � ��������� �� ��������
        foreach (var pair in cardMappings) {
            if (pair.Value == cardUI) {
                pair.Key.UpdateTexture(texture);
            }
        }
    }

    /// <summary>
    /// ����� ������� ��� �������� �����
    /// </summary>
    public void UnregisterCard(Card3DView card3D) {
        if (cardMappings.TryGetValue(card3D, out CardUIView cardUI)) {
            if (dynamicCards.Contains(cardUI)) {
                dynamicCards.Remove(cardUI);
                dynamicRenderingRoom.RemoveCardUI(cardUI);
            } else {
                staticRenderingRoom.RemoveCardUI(cardUI);
            }

            cardMappings.Remove(card3D);
            // ������� ��������� ����� ��� �������� ������� CardUIView ���� �������
        }
    }
}
