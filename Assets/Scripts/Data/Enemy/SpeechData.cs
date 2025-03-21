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
    public List<StoryDialogueData> storyDialogues;
    public List<BaseDialogueData> eventDialogues;
    public float typingSpeed = 1.0f;
}

public class Speaker : IDisposable {
    private readonly DialogueSystem dialogueSystem;
    private readonly GameEventBus eventBus;
    private readonly List<IDialogue> eventDialogues = new List<IDialogue>();

    private readonly Dictionary<int, List<IDialogue>> storyDialogues = new();

    public SpeechData SpeechData { get; }

    public Speaker(SpeechData speechData, DialogueSystem dialogueSystem, GameEventBus eventBus) {
        SpeechData = speechData;
        this.dialogueSystem = dialogueSystem;
        this.eventBus = eventBus;

        eventBus.SubscribeTo<OnRoundStart>(UpdateStoryDialogs);
        Initialize();
    }

    private void Initialize() {
        SetupStoryDialogues();

        foreach (var dialogueData in SpeechData.eventDialogues) {
            var dialogue = dialogueData.CreateDialogue(this, dialogueSystem, eventBus);
            dialogue.Subscribe();
            eventDialogues.Add(dialogue);
        }
    }

    private void SetupStoryDialogues() {
        foreach (var storyDialog in SpeechData.storyDialogues) {
            var dialog = storyDialog.CreateDialogue(this, dialogueSystem, eventBus);
            int activationTurn = storyDialog.triggerOnRound;

            // Використовуємо ?? для перевірки та ініціалізації списку
            if (!storyDialogues.TryGetValue(activationTurn, out var turnDialogues)) {
                storyDialogues[activationTurn] = turnDialogues = new List<IDialogue>();
            }

            turnDialogues.Add(dialog);
        }
    }

    private void UpdateStoryDialogs(ref OnRoundStart eventData) {
        if (storyDialogues.TryGetValue(eventData.RoundNumber, out List<IDialogue> dialogues)) {
            foreach (var dialog in dialogues) {
                dialog.Subscribe();
            }
        }
    }

    public bool TryGetSpeechSound(out AudioClip clip) {
        clip = SpeechData.speechSound;
        return clip != null;
    }

    public void Dispose() {
        foreach (var dialogue in eventDialogues) {
            dialogue.Dispose();
        }
        foreach (var dialogSet in storyDialogues) {
            foreach (var dialogue in dialogSet.Value) {
                dialogue.Dispose();
            }
        }
    }
}

