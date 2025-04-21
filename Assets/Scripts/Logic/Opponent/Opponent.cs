using Cysharp.Threading.Tasks;
using System;
using Zenject;

public interface IMannable {
    public Mana Mana { get; }
}

public class Opponent : IDisposable, IDamageable, IMannable {
    public Action<Opponent> OnDefeat { get; internal set; }
    public Func<BoardSeat, UniTask> OnTookSeat;
    public Func<UniTask> OnRemovedFromSeat;

    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public OpponentData Data { get; private set; }

    [Inject] protected GameEventBus _eventBus;
    public Opponent(OpponentData data) {
        Data = data;
        Health = new Health(Data.Health, this, _eventBus);
        Mana = new Mana(this, Data.Mana, _eventBus);
    }
   
    public async UniTask TakeSeat(BoardSeat boardSeat) {
        if (OnTookSeat != null) {
            await OnTookSeat(boardSeat);
        }
    }

    public async UniTask ClearSeat() {
        if (OnTookSeat != null) {
            await OnRemovedFromSeat();
        }
    }
    public override string ToString() {
        return $"{GetType().Name} {Data.Name} ({Health.CurrentValue}/{Health.TotalValue})";
    }
    public virtual void Dispose() {
        GC.SuppressFinalize(this);
    }
}

