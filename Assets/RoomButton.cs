using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour {
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI text;
    internal void Initialize(RoomData data, Action value) {
        if (data == null) {
            Debug.Log("LOL");
            return;
        }
        text.text = data.roomName;
        button.onClick.AddListener(() => value());
    }
}