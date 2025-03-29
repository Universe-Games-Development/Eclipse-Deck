using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueSet {

    [TextArea(3, 10)]
    public List<string> messages = new();
}

public abstract class BaseDialogueData : ScriptableObject {
    public abstract IDialogue CreateDialogue(Speaker speaker, DialogueSystem dialogueSystem, GameEventBus eventBus);
}

public abstract class EventDialogData<TEvent> : BaseDialogueData where TEvent : IEvent {
    public abstract bool IsMet(TEvent eventData);
    public abstract Dictionary<string, string> GetReplacements(TEvent eventData);
    public abstract DialogueSet GetDialogSet();
}

public abstract class RandomEventDialogueData<TEvent> : EventDialogData<TEvent> where TEvent : IEvent {
    [Header("Dialogue Content")]
    public List<DialogueSet> speeches = new();

    public override DialogueSet GetDialogSet() {
        return speeches.GetRandomElement();
    }

    [Range(0, 1f)] public float probability = 0.3f;
    public override IDialogue CreateDialogue(Speaker speaker, DialogueSystem dialogueSystem, GameEventBus eventBus) {

        return new RandomEventDialogue<TEvent>(this, dialogueSystem, eventBus, speaker);
    }
}

public interface IDialogue : IDisposable {
    BaseDialogueData DialogueData { get; }
    void Subscribe();
    void Unsubscribe();
    bool IsActive { get; }
}

public abstract class BaseDialogue : IDialogue {
    protected readonly DialogueSystem dialogueSystem;
    protected readonly GameEventBus eventBus;
    protected readonly Speaker speaker;
    protected readonly BaseDialogueData baseDialogueData;
    protected bool isActive = false;

    public BaseDialogueData DialogueData => baseDialogueData;
    public bool IsActive => isActive;

    protected BaseDialogue(BaseDialogueData dialogueData, DialogueSystem dialogueSystem, GameEventBus eventBus, Speaker speaker) {
        this.baseDialogueData = dialogueData;
        this.dialogueSystem = dialogueSystem;
        this.eventBus = eventBus;
        this.speaker = speaker;
    }

    public abstract void Subscribe();
    public abstract void Unsubscribe();

    public virtual void Dispose() {
        Unsubscribe();
    }
}

public class RandomEventDialogue<TEvent> : BaseDialogue where TEvent : IEvent {
    private readonly RandomEventDialogueData<TEvent> typedDialogueData;
    protected int activationCount = 0;
    public RandomEventDialogue(RandomEventDialogueData<TEvent> dialogueData, DialogueSystem dialogueSystem, GameEventBus eventBus, Speaker speaker)
        : base(dialogueData, dialogueSystem, eventBus, speaker)
        {
        typedDialogueData = dialogueData;
    }

    public override void Subscribe() {
        if (isActive) return;
        eventBus.SubscribeTo<TEvent>(OnEventBegin);
        isActive = true;
    }

    public override void Unsubscribe() {
        if (!isActive) return;
        eventBus.UnsubscribeFrom<TEvent>(OnEventBegin);
        isActive = false;
    }

    protected void OnEventBegin(ref TEvent eventData) {
        if (!isActive) return;

        if (!typedDialogueData.IsMet(eventData)) return;

        if (typedDialogueData.probability < 1.0f && UnityEngine.Random.value > typedDialogueData.probability) {
            Debug.Log($"Dialogue didn't trigger due to probability: {typedDialogueData.name}");
            return;
        }

        // Get dialogue pages for this event
        DialogueSet dialogueSet = typedDialogueData.GetDialogSet();

        // Apply replacements
        List<string> processedMessages = new List<string>();
        var replacements = typedDialogueData.GetReplacements(eventData);

        foreach (string page in dialogueSet.messages) {
            string processedPage = page;
            foreach (var replacement in replacements) {
                processedPage = processedPage.Replace($"{{{replacement.Key}}}", replacement.Value);
            }
            processedMessages.Add(processedPage);
        }

        dialogueSystem.ShowDialogue(speaker, new Queue<string>(processedMessages));

        activationCount++;
    }
}


