using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class RoomNode : MonoBehaviour {
    
    public RoomType RoomType { get; private set; }
    public List<RoomNode> connectedRooms;

    public LineRenderer lineRenderer;
    [SerializeField] private SpriteRenderer roomSprite;
    public event Action<RoomNode> OnRoomSelected;

    private void OnMouseDown() {
        OnRoomSelected?.Invoke(this);
    }
    private void Awake() {
        if (lineRenderer == null) {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (roomSprite == null) {
            roomSprite = GetComponent<SpriteRenderer>();
        }
    }

    // Можна додати метод для візуалізації вибору кімнати
    public void Highlight(bool isHighlighted) {
        if (roomSprite != null) {
            roomSprite.color = isHighlighted ?
                new Color(1f, 1f, 1f, 1f) :
                new Color(0.7f, 0.7f, 0.7f, 0.7f);
        }
    }
}


public enum RoomType {
    Entrance,    // Вхід (завжди перший)
    Enemy,       // Кімната з ворогом
    Treasure,    // Кімната скарбів
    Altar,       // Кімната алтаря
    Shop,        // Магазин
    Boss,        // Бос (завжди передостанній)
    Exit         // Вихід (завжди останній)
}
