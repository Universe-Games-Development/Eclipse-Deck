using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Map/Room")]
public class RoomData : ScriptableObject {
    public string roomName;
    public GameObject Prefab; // ������ �������
    public RoomType type;
    public float baseChance; // ������� ���� ���������
    public float repeatPenalty; // ������� �������� ����� ��� ��������� � ���� ���� (0.5 = 50%)
    public Color roomColor; // ���� ��� ����������
}
