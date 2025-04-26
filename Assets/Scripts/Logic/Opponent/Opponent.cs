using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;



public class Opponent : IDisposable, IDamageable, IMannable {
    public Action<Opponent> OnDefeat { get; internal set; }

    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public OpponentData Data { get; private set; }

    [Inject] protected GameEventBus _eventBus;
    public Opponent(OpponentData data) {
        Data = data;
        Health = new Health(Data.Health, this, _eventBus);
        Mana = new Mana(this, Data.Mana, _eventBus);
    }

    public override string ToString() {
        return $"{GetType().Name} {Data.Name} ({Health.CurrentValue}/{Health.TotalValue})";
    }
    public virtual void Dispose() {
        GC.SuppressFinalize(this);
    }
}

public class Player : Opponent {
    
    public PlayerData PlayerData => (PlayerData)base.Data;

    public Player(PlayerData data) : base(data) {

    }
}


public class Enemy : Opponent {
    private Speaker speech;
    [Inject] private TurnManager _turnManager;
    [Inject] protected OpponentRegistrator opponentRegistrator;

    public Enemy(OpponentData opponentData, DialogueSystem dialogueSystem, GameEventBus eventBus)
        : base(opponentData) {
        SpeechData speechData = opponentData.speechData;
        if (speechData != null) {
            speech = new Speaker(speechData, this, dialogueSystem, eventBus);
        }
    }

    private async UniTask PerformTestTurn() {
        await UniTask.Delay(1500);
        _turnManager.EndTurnRequest();
    }
}