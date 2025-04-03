using System;

public abstract class RoomActivity : IDisposable {
    protected bool _blocksRoomClear = false;
    public bool BlocksRoomClear => _blocksRoomClear;
    public Action OnActivityCompleted;
    public string Name { get; private set; }
    public abstract void Initialize(Room room);
    public void CompleteActivity() {
        OnActivityCompleted?.Invoke();
    }

    public RoomActivity SetName(string name) {
        Name = name;
        return this;
    }

    public virtual void Dispose() { }
}
