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
}

public class Speech : IDisposable {
    private DialogueSystem dialogueSystem;
    public SpeechData speechData;
    private GameEventBus eventBus;
    List<IDialogue> dialogues = new();

    public Speech(DialogueSystem dialogueSystem, SpeechData speechData, GameEventBus eventBus) {
        this.dialogueSystem = dialogueSystem;
        this.speechData = speechData;
        this.eventBus = eventBus;
        SetupDialogues();
    }

    private void SetupDialogues() {
        foreach (var dialogData in speechData.dialogueDatas) {
            var dialog = dialogData.CreateDialog(this, dialogueSystem, eventBus);
            dialog.Subscribe();
            dialogues.Add(dialog);
        }
    }

    public void Dispose() {
        foreach (var dialog in dialogues) {
            dialog.Dispose();
        }
    }

    public bool TryGetSpeechSound(out AudioClip clip) {
        clip = speechData.speechSound;
        return clip != null;
    }

}


