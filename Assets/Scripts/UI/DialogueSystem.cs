using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class DialogueSystem : MonoBehaviour {
    [Header("Header")]
    [SerializeField] private Image characterSprite;
    [SerializeField] private TextMeshProUGUI characterName;

    [Header("Body")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject dialoguePanel; // Panel containing the whole dialogue UI

    [Header("Settings")]
    [SerializeField] private float letterDelay = 0.05f;
    [SerializeField] private float delayBetweenMessages = 0.5f; // Delay after message is fully displayed

    private Speech currentSpeech;
    private Queue<string> remainingMessages = new Queue<string>();
    private bool isTyping = false;
    private bool isWaitingForInput = false;
    private CancellationTokenSource cts;

    [Inject]
    private AudioManager audioManager;

    private void Start() {
        // Hide dialogue panel at start
        if (dialoguePanel != null) {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update() {
        // Check for input
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            HandleInput();
        }
    }

    private void HandleInput() {
        if (isTyping) {
            // If typing, complete the current text immediately
            cts?.Cancel();
            cts = new CancellationTokenSource();
            CompleteCurrentText().Forget();
        } else if (isWaitingForInput) {
            // If waiting for input, proceed to next message
            isWaitingForInput = false;
            DisplayNextMessage().Forget();
        }
    }

    public void SetMessages(Speech speech, Queue<string> dialogMessages) {
        cts?.Cancel();
        cts = new CancellationTokenSource();

        currentSpeech = speech;
        remainingMessages = new Queue<string>(dialogMessages);

        UpdateCharacterInfo(speech.speechData);

        // Open dialogue panel
        OpenDialoguePanel();

        // Start displaying messages
        DisplayNextMessage().Forget();
    }

    private void UpdateCharacterInfo(SpeechData data) {
        characterName.text = data.characterName;
        characterSprite.sprite = data.characterPortrait;
    }

    private async UniTask DisplayNextMessage() {
        if (remainingMessages.Count > 0) {
            string message = remainingMessages.Dequeue();
            await TypeText(message, cts.Token);

            // After typing is complete, wait for player input
            isWaitingForInput = true;
        } else {
            // No more messages, close the dialogue
            CloseDialoguePanel();
        }
    }

    private async UniTask TypeText(string text, CancellationToken ct) {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in text) {
            if (ct.IsCancellationRequested)
                break;

            dialogueText.text += letter;

            if (audioManager != null && currentSpeech != null && currentSpeech.TryGetSpeechSound(out AudioClip clip)) {
                audioManager.PlaySound(clip);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(letterDelay), cancellationToken: ct);
        }

        isTyping = false;
    }

    private async UniTask CompleteCurrentText() {
        if (isTyping) {
            // Get the current message that was being typed
            string currentMessage = remainingMessages.Count > 0 ?
                remainingMessages.Peek() : dialogueText.text;

            // Display it fully
            dialogueText.text = currentMessage;
            isTyping = false;

            // Wait briefly before allowing next input
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: cts.Token);
            isWaitingForInput = true;
        }
    }

    public void OpenDialoguePanel() {
        if (dialoguePanel != null) {
            dialoguePanel.SetActive(true);
        }
    }

    public void CloseDialoguePanel() {
        if (dialoguePanel != null) {
            dialoguePanel.SetActive(false);
        }

        // Clear current state
        cts?.Cancel();
        cts = null;
        currentSpeech = null;
        remainingMessages.Clear();
        isTyping = false;
        isWaitingForInput = false;
    }

    // Public method to close dialogue from outside
    public void ForceCloseDialogue() {
        CloseDialoguePanel();
    }
}