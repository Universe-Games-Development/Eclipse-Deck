using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour {
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI text;
    internal void Initialize(Room room, Action value) {
        if (room == null) {
            Debug.Log("LOL room button not initialized text");
            return;
        }
        text.text = room.GetName();
        button.onClick.AddListener(() => value());
    }
}