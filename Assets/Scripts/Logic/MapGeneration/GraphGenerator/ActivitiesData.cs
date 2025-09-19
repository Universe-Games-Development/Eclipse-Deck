using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonActivityData", menuName = "Dungeon/DungeonActivityData")]
public class ActivitiesData : ScriptableObject {
    [Header("Base Activities")]

    public ExitActivityData exitLocationActivity;
    public ActivityData locationEnteracnceActivity;
    public BossActivityData bossActivity;

    [Header("Random Activities")]
    public List<ActivityData> commonActivities;

}