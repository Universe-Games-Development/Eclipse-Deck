using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;



public class Character : UnitModel, IDisposable {
    public Action<Character> OnDefeat { get; internal set; }

    
    public CharacterData Data { get; private set; }

    
    public Character(CharacterData data) {
        Data = data;
        
    }

    
    public virtual void Dispose() {
        GC.SuppressFinalize(this);
    }
}

public class Player : Character {
    
    public PlayerData PlayerData => (PlayerData)base.Data;

    public Player(PlayerData data) : base(data) {
        
    }
}


public class Enemy : Character {
    private Speaker speech;
    [Inject] private TurnManager _turnManager;
    [Inject] protected OpponentRegistrator opponentRegistrator;

    public Enemy(CharacterData opponentData, DialogueSystem dialogueSystem, IEventBus<IEvent> eventBus)
        : base(opponentData) {
        SpeechData speechData = opponentData.speechData;
        if (speechData != null) {
            speech = new Speaker(speechData, this, dialogueSystem, eventBus);
        }
    }
}