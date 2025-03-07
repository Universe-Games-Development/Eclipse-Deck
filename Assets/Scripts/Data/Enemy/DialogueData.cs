using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseDialogueData : ScriptableObject {
    public abstract IDialogue CreateDialog(Speech speech, DialogueSystem dialogueSystem, GameEventBus eventBus);
}

public abstract class DialogueData<TEvent> : BaseDialogueData where TEvent : IEvent {
    [Header("Dialogue Pages")]
    [TextArea(3, 5)]
    public List<string> pages = new List<string>();

    public abstract Dictionary<string, string> GetReplacements(TEvent eventData);

    public override IDialogue CreateDialog(Speech speech, DialogueSystem dialogueSystem, GameEventBus eventBus) {
        return new Dialogue<TEvent>(speech, this, dialogueSystem, eventBus);
    }

    public List<string> BuildMessages(TEvent eventData) {
        Dictionary<string, string> replacements = GetReplacements(eventData);
        List<string> formattedMessages = new List<string>();

        foreach (string page in pages) {
            string formattedPage = page;
            foreach (var replacement in replacements) {
                formattedPage = formattedPage.Replace($"{{{replacement.Key}}}", replacement.Value);
            }
            formattedMessages.Add(formattedPage);
        }

        return formattedMessages;
    }

    public abstract bool IsMet(TEvent eventData);
}

public interface IDialogue : IDisposable {
    void Subscribe();
    void Unsubscribe();
}

public class Dialogue<TEvent> : EventListener<TEvent>, IDialogue where TEvent : IEvent {
    private DialogueData<TEvent> dialogueData;
    private DialogueSystem dialogueSystem;
    private Speech speech;

    public Dialogue(Speech speech, DialogueData<TEvent> dialogueData, DialogueSystem dialogueSystem, GameEventBus eventBus)
        : base(eventBus) {
        this.dialogueData = dialogueData;
        this.dialogueSystem = dialogueSystem;
        this.speech = speech;
        
        Subscribe();
    }

    protected override void OnEventBegin(ref TEvent eventData) {
        if (!dialogueData.IsMet(eventData)) {
            Debug.Log("Event not met");
            return;
        }
        Debug.Log("Event met");
        List<string> messages = dialogueData.BuildMessages(eventData);
        Queue<string> dialogMessages = new Queue<string>(messages);
        dialogueSystem.SetMessages(speech, dialogMessages);
        Unsubscribe();

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
