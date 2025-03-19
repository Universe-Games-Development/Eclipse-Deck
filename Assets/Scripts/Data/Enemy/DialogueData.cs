using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseDialogueData : ScriptableObject {
    public abstract IDialogue CreateDialogue(Speaker speaker, DialogueSystem dialogueSystem, GameEventBus eventBus);
}

public class DialogueData<TEvent> : BaseDialogueData where TEvent : IEvent {
    [Header("Dialogue Pages")]
    [TextArea(3, 5)]
    public List<string> pages = new List<string>();

    [Header("Activation Settings")]
    public bool isGlobal = false;       
    public int activationTurn = -1;           
    [Header("Activation Settings")]
    public int maxActivations = 1;           
    [Range (0, 1f)] public float probability = 1.0f;

    private void OnValidate() {
        isGlobal = activationTurn < 0;
    }

    public virtual bool IsMet(TEvent eventData) {
        return true;
    }

    public virtual Dictionary<string, string> GetReplacements(TEvent eventData) {
        return new Dictionary<string, string>();
    }

    public override IDialogue CreateDialogue(Speaker speaker, DialogueSystem dialogueSystem, GameEventBus eventBus) {
        return new Dialogue<TEvent>(this, dialogueSystem, eventBus, speaker);
    }
}

public interface IDialogue : IDisposable {
    void Activate();
    void Deactivate();
    bool IsActive { get; }
    bool IsGlobal { get; }
    bool IsEligibleForTurn(int turnCount);
    BaseDialogueData DialogueData { get; }
}

public class Dialogue<TEvent> : IDialogue where TEvent : IEvent {
    private readonly DialogueData<TEvent> dialogueData;
    private readonly DialogueSystem dialogueSystem;
    private readonly GameEventBus eventBus;
    private readonly Speaker speaker;
    private int activationCount = 0;
    private bool isSubscribed = false;

    public BaseDialogueData DialogueData => dialogueData;
    public bool IsActive { get; private set; } = false;
    public bool IsGlobal { get { return dialogueData.isGlobal; } }

    public Dialogue(DialogueData<TEvent> dialogueData, DialogueSystem dialogueSystem, GameEventBus eventBus, Speaker speaker) {
        this.dialogueData = dialogueData;
        this.dialogueSystem = dialogueSystem;
        this.eventBus = eventBus;
        this.speaker = speaker;
    }

    public void Activate() {
        if (IsActive) return;

        IsActive = true;

        if (dialogueData.isGlobal) {
            Subscribe();
        }
    }

    public void Deactivate() {
        if (!IsActive) return;

        IsActive = false;
        Unsubscribe();
    }

    public bool IsEligibleForTurn(int turnCount) {
        if (dialogueData.isGlobal) return true;

        if (dialogueData.activationTurn == -1) return true;

        return dialogueData.activationTurn == turnCount;
    }

    private void Subscribe() {
        if (isSubscribed) return;
        eventBus.SubscribeTo<TEvent>(OnEventTriggered);
        isSubscribed = true;
    }

    private void Unsubscribe() {
        if (!isSubscribed) return;
        eventBus.UnsubscribeFrom<TEvent>(OnEventTriggered);
        isSubscribed = false;
    }

    private void OnEventTriggered(ref TEvent eventData) {
        if (!IsActive) return;

        if (!dialogueData.IsMet(eventData)) return;

        if (dialogueData.maxActivations > 0 && activationCount >= dialogueData.maxActivations) {
            Deactivate();
            return;
        }

        if (dialogueData.probability < 1.0f && UnityEngine.Random.value > dialogueData.probability) {
            Debug.Log($"Діалог не спрацював через ймовірність: {dialogueData.name}");
            return;
        }

        // Building message with replacements
        List<string> processedMessages = new List<string>();
        var replacements = dialogueData.GetReplacements(eventData);

        foreach (string page in dialogueData.pages) {
            string processedPage = page;
            foreach (var replacement in replacements) {
                processedPage = processedPage.Replace($"{{{replacement.Key}}}", replacement.Value);
            }
            processedMessages.Add(processedPage);
        }

        dialogueSystem.ShowDialogue(speaker, new Queue<string>(processedMessages));

        activationCount++;
    }

    public void Dispose() {
        Deactivate();
    }
}


public abstract class EventListener<TEvent> : IDisposable where TEvent : IEvent {
    private GameEventBus eventBus;
    private bool isSubscribedToEvent = false;

    public EventListener(GameEventBus eventBus) {
        this.eventBus = eventBus;
    }

    public void Subscribe() {
        if (isSubscribedToEvent) return;
        eventBus.SubscribeTo<TEvent>(OnEventBegin);
        isSubscribedToEvent = true;
    }

    protected abstract void OnEventBegin(ref TEvent eventData);

    public void Unsubscribe() {
        if (!isSubscribedToEvent) return;
        eventBus.UnsubscribeFrom<TEvent>(OnEventBegin);
        isSubscribedToEvent = false;
    }

    public void Dispose() {
        Unsubscribe();
    }
}
