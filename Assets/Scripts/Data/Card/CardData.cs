using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class CardData : ScriptableObject {
    [Header("Global Card Settings")]
    public int cost;

    [Header("Card UI")]
    public string Name;
    public string Description;
    public string AuthorName;
    public Sprite Portait;
    public Sprite Background;

    [Header("Logic")]
    public string resourseId;
    public Rarity Rarity;
   

    [Header("Rarity Info (Auto-Generated)")]
    [SerializeField] private float spawnChance; // ����� ��������, ����������� �����������
    [SerializeField] private Color rarityColor; // �������� ���� � ��������� ��� ����������
    [SerializeField] private string rarityDisplayName; // ����������� �����

    [Header("Operations")]
    public List<OperationData> operationsData = new List<OperationData>();

    private void OnValidate() {
        // �������� ��������� ID ���� �������
        if (string.IsNullOrEmpty(resourseId)) {
            resourseId = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        // ��������� ��� ������ ����� ������
        UpdateRarityData();
    }

    private void UpdateRarityData() {
        spawnChance = RarityUtility.GetSpawnChance(Rarity);
        rarityColor = RarityUtility.GetRarityColor(Rarity);
        rarityDisplayName = RarityUtility.GetRarityDisplayName(Rarity);
    }

}
