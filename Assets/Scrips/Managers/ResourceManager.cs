using System.Collections.Generic;
using UnityEngine;

public enum ResourceType {
    NONE,
    CARDS,
    ENEMIES
}

public class ResourceManager : MonoBehaviour {
    [Header("Card Data")]
    public List<CardSO> cardDataList = new List<CardSO>();

    [SerializeField]
    private string cardResourcePath = "Cards"; // Шлях до ресурсів з картами

    private void OnEnable() {
        LoadResources(ref cardDataList, cardResourcePath, ResourceType.CARDS);
    }

    private void LoadResources<T>(ref List<T> targetList, string path, ResourceType resourceType = ResourceType.NONE) where T : Object {
        // Очищаємо список перед завантаженням
        targetList.Clear();

        // Завантажуємо всі ресурси за заданим шляхом
        T[] resources = Resources.LoadAll<T>(path);

        if (resources != null && resources.Length > 0) {
            targetList.AddRange(resources);
            Debug.Log($"[{resourceType}] {resources.Length} resources loaded from '{path}'.");
        } else {
            Debug.LogWarning($"[{resourceType}] No resources found at path: {path}");
        }
    }

    public CardSO GetCardByID(string cardID) {
        return cardDataList.Find(card => card.id == cardID);
    }

    public List<CardSO> GetAllCards() {
        return cardDataList;
    }

    public CardSO GetRandomCard() {
        if (cardDataList.Count == 0) {
            Debug.LogWarning("Card data list is empty.");
            return null;
        }

        int randomIndex = Random.Range(0, cardDataList.Count);
        return cardDataList[randomIndex];
    }

}
