using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonsTestHandler : MonoBehaviour {
    private List<Button> buttonsList = new List<Button>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        // Збираємо всі кнопки в дочірніх об'єктах
        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button button in buttons) {
            // Призначаємо івент
            button.onClick.AddListener(() => OnButtonClick(button.name));
            // Додаємо кнопку в список
            buttonsList.Add(button);
        }
    }

    // Виконується при натисканні кнопки
    void OnButtonClick(string buttonName) {
        Debug.Log("Кнопку " + buttonName + " було натиснуто.");
    }

    // Викликається при відключенні об'єкта
    void OnDisable() {
        // Вимикаємо всі івенти при відключенні об'єкта
        foreach (Button button in buttonsList) {
            button.onClick.RemoveAllListeners();
        }
    }
}
