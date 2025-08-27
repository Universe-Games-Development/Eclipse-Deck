using System.IO;
using Zenject;

public interface IRoomActivityFactory {
    RoomActivity CreateActivity(DungeonGraph graph, DungeonNode node, Room room);
    void UpdateActivityData(ActivitiesData activitiesData);
}

public class RoomActivityFactory : IRoomActivityFactory {
    private WeightedRandomizer<ActivityData> weightedRandomizer = new();
    private ActivitiesData _activitiesData;
    [Inject] DiContainer container;
    
    public RoomActivityFactory(ActivitiesData activitiesData) {
        UpdateActivityData(activitiesData);
    }

    public RoomActivityFactory() {
    }

    public void UpdateActivityData(ActivitiesData activitiesData) {
        if (activitiesData == null) {
            throw new InvalidDataException("activitiesData is null");
        }
        _activitiesData = activitiesData;
        weightedRandomizer.UpdateItems(_activitiesData.commonActivities);
    }

    public RoomActivity CreateActivity(DungeonGraph graph, DungeonNode node, Room room) {
        ActivityData activityData = null;
        int level = node.level;

        if (level == 0) {
            activityData = _activitiesData.locationEnteracnceActivity;
        } else if (level == graph.GetLevelNodes().Count - 2) { // preEndLevel
            activityData = _activitiesData.bossActivity;
        } else if (level == graph.GetLevelNodes().Count - 1) { // endLevel
            activityData = _activitiesData.exitLocationActivity;
        } else {
            activityData = weightedRandomizer.GetRandomItem();
        }

        if (activityData == null) throw new InvalidDataException("Activity data is null");
        return activityData.CreateActivity(container).SetName(activityData.Name);
    }
}
