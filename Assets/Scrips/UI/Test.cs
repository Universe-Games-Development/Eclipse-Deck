using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        // �������� ������ ��������, �� �� ����� �������
        List<GameObject> clickableElements = GetClickableElements();
        Debug.Log("Clickable Elements Count: " + clickableElements.Count);
        foreach (var element in clickableElements) {
            Debug.Log("Clickable Element: " + element.name);
        }
    }

    private List<GameObject> GetClickableElements() {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Pointer.current.position.value;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        List<GameObject> clickableElements = new List<GameObject>();
        foreach (var result in results) {
            if (result.gameObject.GetComponent<IPointerClickHandler>() != null) {
                // ������ ������� �� ������ �����������
                clickableElements.Add(result.gameObject);
            }
        }
        return clickableElements;
    }
}