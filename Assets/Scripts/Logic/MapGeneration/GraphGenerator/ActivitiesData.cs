using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DungeonActivityData", menuName = "Dungeon/DungeonActivityData")]
public class ActivitiesData : ScriptableObject {
    [Header("Base Activities")]

    public ActivityData exitLocationActivity;
    public ActivityData locationEnteracnceActivity;
    public ActivityData bossActivity;

    [Header("Random Activities")]
    public List<ActivityData> commonActivities;
    
}