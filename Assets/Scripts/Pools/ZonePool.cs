[System.Serializable]
public class ZonePool : ComponentPool<ZoneView> {
    protected override void OnTakeFromPool(ZoneView item) {
        base.OnTakeFromPool(item);
    }
}
