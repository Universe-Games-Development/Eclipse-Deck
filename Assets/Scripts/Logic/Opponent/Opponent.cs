using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public interface IMannable {
    public Mana Mana { get; }
}

public class Opponent : IDisposable, IDamageable, IGameUnit, IMannable {
    public Action<Opponent> OnDefeat { get; internal set; }
    
    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public OpponentData Data { get; private set; }
    public event Action<SummonEvent> OnUnitDeployed;

    public Opponent ControlOpponent => this;

    [Inject] protected GameEventBus _eventBus;
    public Opponent(OpponentData data) {
        Data = data;
        Health = new Health(Data.Health, this, _eventBus);
        Mana = new Mana(this, Data.Mana, _eventBus);
    }

    public void BeginBattle() {
        // Initialize battle logic here
        OnUnitDeployed?.Invoke(new SummonEvent(this));
    }

    public virtual void Dispose() {
        GC.SuppressFinalize(this);
    }

    public override string ToString() {
        return $"{GetType().Name} {Data.Name} ({Health.CurrentValue}/{Health.TotalValue})";
    }

    
    public Func<BoardSeat, UniTask> OnTookSeat;
    public Func<UniTask> OnRemovedFromSeat;
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
}

