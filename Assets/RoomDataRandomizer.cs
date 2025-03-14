using ModestTree;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomDataRandomizer {
    private List<RoomData> roomsFillers;
    private Dictionary<RoomData, float> weightPoints = new Dictionary<RoomData, float>();
    private System.Random rand;

    public RoomDataRandomizer(List<RoomData> roomsFillers, System.Random rand) {
        if (roomsFillers.IsEmpty()) {
            Debug.LogError("Empty room list to generate rooms");
            return;
        }
        this.roomsFillers = roomsFillers;
        this.rand = rand;
        ValidateRoomList();
        GenerateWeightPoints();
    }

    /// <summary>
    /// Dublicates validation
    /// </summary>
    private void ValidateRoomList() {
        HashSet<RoomData> roomSet = new HashSet<RoomData>();
        foreach (var room in roomsFillers) {
            if (!roomSet.Add(room)) {
                Debug.LogWarning($"Found duplicate for: {room.roomName}");
            }
        }
    }

    /// <summary>
    /// Generates cumulative weights for rooms chances
    /// </summary>
    private void GenerateWeightPoints() {
        float totalWeight = roomsFillers.Sum(room => room.baseChance);
        if (totalWeight <= 0) {
            Debug.LogError("Total weight must be greater than zero!");
            return;
        }

        weightPoints.Clear();
        float cumulative = 0f;
        foreach (var room in roomsFillers) {
            float normalizedWeight = room.baseChance / totalWeight;
            cumulative += normalizedWeight;
            weightPoints.Add(room, cumulative);
        }
    }


    /// <summary>
    /// Returns random room by value 0 - 1
    /// </summary>
    /// <param name="value">value from 0 to 1</param>
    /// <returns>Random Room</returns>
    public RoomData GetRandomRoomData() {
        if (weightPoints.IsEmpty()) {
            return null;
        }
        float randomWeight = (float)rand.NextDouble();
        List<KeyValuePair<RoomData, float>> roomList = weightPoints.ToList();
        int left = 0;
        int right = roomList.Count - 1;

        while (left <= right) {
            int mid = (left + right) / 2;
            if (randomWeight <= roomList[mid].Value) {
                if (mid == 0 || randomWeight > roomList[mid - 1].Value) {
                    return roomList[mid].Key;
                }
                right = mid - 1;
            } else {
                left = mid + 1;
            }
        }

        return roomList.Last().Key;
    }
}

