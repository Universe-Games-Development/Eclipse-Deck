using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;



public class Opponent : IDisposable {
    public Action<Opponent> OnDefeat { get; internal set; }

    
    public OpponentData Data { get; private set; }

    
    public Opponent(OpponentData data) {
        Data = data;
        
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
}