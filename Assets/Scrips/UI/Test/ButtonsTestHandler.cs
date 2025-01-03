using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonsTestHandler : MonoBehaviour {
    private List<Button> buttonsList = new List<Button>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        // ������� �� ������ � ������� ��'�����
        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button button in buttons) {
            // ���������� �����
            button.onClick.AddListener(() => OnButtonClick(button.name));
            // ������ ������ � ������
            buttonsList.Add(button);
        }
    }

    // ���������� ��� ��������� ������
    void OnButtonClick(string buttonName) {
        Debug.Log("������ " + buttonName + " ���� ���������.");
    }

    // ����������� ��� ��������� ��'����
    void OnDisable() {
        // �������� �� ������ ��� ��������� ��'����
        foreach (Button button in buttonsList) {
            button.onClick.RemoveAllListeners();
        }
    }
}
