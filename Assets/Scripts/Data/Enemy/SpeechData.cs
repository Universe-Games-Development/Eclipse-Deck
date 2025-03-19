using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpeech", menuName = "Dialogues/Speech")]
public class SpeechData : ScriptableObject {
    [Header("Character Metadata")]
    public AudioClip speechSound;
    public string characterName; 
    public Sprite characterPortrait;

    [Header("Dialogue Data")]
    public List<BaseDialogueData> dialogueDatas;
    internal float typingSpeed = 1.0f;
}

public class Speaker : IDisposable {
    private readonly DialogueSystem dialogueSystem;
    private readonly GameEventBus eventBus;
    private readonly TurnManager turnManager;
    private readonly List<IDialogue> allDialogues = new List<IDialogue>();

    public SpeechData SpeechData { get; }

    public Speaker(SpeechData speechData, DialogueSystem dialogueSystem, TurnManager turnManager, GameEventBus eventBus) {
        SpeechData = speechData;
        this.dialogueSystem = dialogueSystem;
        this.turnManager = turnManager;
        this.eventBus = eventBus;

        Initialize();
    }

    private void Initialize() {
        foreach (var dialogueData in SpeechData.dialogueDatas) {
            var dialogue = dialogueData.CreateDialogue(this, dialogueSystem, eventBus);
            if (dialogue.IsGlobal) {
                dialogue.Activate();
            }
            allDialogues.Add(dialogue);
        }

        eventBus.SubscribeTo<OnTurnStart>(OnTurnChanged);

        UpdateDialoguesForTurn(turnManager.TurnCounter);
    }

    private void OnTurnChanged(ref OnTurnStart eventData) {
        UpdateDialoguesForTurn(eventData.TurnCount);
    }

    private void UpdateDialoguesForTurn(int turnCount) {
        foreach (var dialogue in allDialogues) {
            if (dialogue.IsEligibleForTurn(turnCount)) {
                dialogue.Activate();
            } else {
                dialogue.Deactivate();
            }
        }
    }

    public bool TryGetSpeechSound(out AudioClip clip) {
        clip = SpeechData.speechSound;
        return clip != null;
    }

    public void Dispose() {
        foreach (var dialogue in allDialogues) {
            dialogue.Dispose();
        }

        eventBus.UnsubscribeFrom<OnTurnStart>(OnTurnChanged);
    }
}

