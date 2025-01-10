using UnityEngine;

[CreateAssetMenu(fileName = "MapInfo", menuName = "Map/MapInfo", order = 1)]
public class MapInfoSO : ScriptableObject {
    public int totalRooms; // �������� ������� �����
    public int numberOfShops; // ʳ������ ��������
    public int numberOfAltars; // ʳ������ ������
    public int numberOfEnemies; // ʳ������ ����� � �������� (��� ����)
    public int numberOfTresures; // ʳ������ ����� � ����������
}
