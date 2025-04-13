using System;

public interface IMannable {
    public Mana Mana { get; }
}

public class Opponent : IDisposable, IHealthEntity, IAbilityOwner, IMannable {
    public Action<Opponent> OnDefeat { get; internal set; }
    
    public IHealth Health { get; private set; }
    public Mana Mana { get; private set; }
    public OpponentData Data { get; private set; }

    protected GameEventBus _eventBus;
    public Opponent(GameEventBus eventBus) {
        _eventBus = eventBus;
    }

    public void SetData(OpponentData data) {
        Data = data;
        Stat healthStat = new(Data.Health);
        Stat manaStat = new(Data.Mana);
        Health = new Health(this, healthStat, _eventBus);
        Mana = new Mana(this, manaStat, _eventBus);
        
    }
    public virtual void Dispose() {
        GC.SuppressFinalize(this);
    }

    public override string ToString() {
        return $"{GetType().Name} {Data.Name} ({Health.Current}/{Health.Max})";
    }
}

