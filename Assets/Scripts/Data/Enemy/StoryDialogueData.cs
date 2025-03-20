using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryDialogue", menuName = "Dialogues/StoryDialogue")]
public class StoryDialogueData : BaseDialogueData {
    [Header("Round Activation Settings")]
    public int triggerOnRound = 1;
    public bool triggerOnPlayerTurn = true;
    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    public List<string> pages = new List<string>();

    public Dictionary<string, string> GetReplacements(OnTurnStart turnStartData, int roundCount) {
        return new Dictionary<string, string> {
            {
            "roundCount", roundCount.ToString()
            },
            {
            "turnCount", turnStartData.TurnNumber.ToString()
            }
        };
    }

    public override IDialogue CreateDialogue(Speaker speaker, DialogueSystem dialogueSystem, GameEventBus eventBus) {
        return new StoryDialogue(this, dialogueSystem, eventBus, speaker);
    }

    public Queue<string> GetContextPages(OnTurnStart turnStartData, int roundCount) {
        List<string> processedMessages = new List<string>();
        Dictionary<string, string> replacements = GetReplacements(turnStartData, roundCount);

        foreach (string page in pages) {
            string processedPage = page;
            foreach (var replacement in replacements) {
                processedPage = processedPage.Replace($"{{{replacement.Key}}}", replacement.Value);
            }
            processedMessages.Add(processedPage);
        }
        return new Queue<string>(processedMessages);
    }
}

public class StoryDialogue : BaseDialogue {
    private readonly StoryDialogueData storyDialogueData;

    public StoryDialogue(StoryDialogueData dialogueData, DialogueSystem dialogueSystem, GameEventBus eventBus, Speaker speaker)
        : base(dialogueData, dialogueSystem, eventBus, speaker) {
        storyDialogueData = dialogueData;
    }

    public override void Subscribe() {
        if (isActive) return;
        eventBus.SubscribeTo<OnRoundStart>(OnRoundStart);
        isActive = true;
    }

    public override void Unsubscribe() {
        if (!isActive) return;
        eventBus.UnsubscribeFrom<OnRoundStart>(OnRoundStart);
        isActive = false;
    }

    private void OnRoundStart(ref OnRoundStart eventData) {
        if (eventData.RoundNumber != storyDialogueData.triggerOnRound) {
            return;
        }
        eventBus.SubscribeTo<OnTurnStart>(OnTurnStart);
        // Since story dialogues only trigger once per round, we can unsubscribe after triggering
        Unsubscribe();
    }

    private void OnTurnStart(ref OnTurnStart eventData) {
        bool isPlayerTurn = eventData.StartingOpponent is Player;
        if (isPlayerTurn != storyDialogueData.triggerOnPlayerTurn) return;

        Queue<string> messages = storyDialogueData.GetContextPages(eventData, storyDialogueData.triggerOnRound);
        dialogueSystem.ShowDialogue(speaker, messages);

        eventBus.UnsubscribeFrom<OnTurnStart>(OnTurnStart);
    }
}