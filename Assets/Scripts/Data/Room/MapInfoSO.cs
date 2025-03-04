using UnityEngine;

[CreateAssetMenu(fileName = "MapInfo", menuName = "Map/MapInfo", order = 1)]
public class MapInfoSO : ScriptableObject {
    public int totalRooms; // Загальна кількість кімнат
    public int numberOfShops; // Кількість магазинів
    public int numberOfAltars; // Кількість алтарів
    public int numberOfEnemies; // Кількість кімнат з ворогами (без боса)
    public int numberOfTresures; // Кількість кімнат з нагородами
}
