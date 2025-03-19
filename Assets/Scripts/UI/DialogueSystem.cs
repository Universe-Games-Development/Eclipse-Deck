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
    

    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject continueIndicator;

    [Header("Settings")]
    [SerializeField] private float letterDelay = 0.05f;
    [SerializeField] private float delayBetweenMessages = 0.5f; // Delay after message is fully displayed
    private string currentMessage;

    private Speaker currentSpeech;
    private Queue<string> remainingMessages = new Queue<string>();
    private bool isTyping = false;
    private bool isWaitingForInput = false;
    private CancellationTokenSource cts;

    [Inject] CommandManager commandManager;
    [Inject] private AudioManager audioManager;

    private void Start() {
        // Hide dialogue panel at start
        if (dialoguePanel != null) {
            dialoguePanel.SetActive(false);
        }
    }

    public void ShowDialogue(Speaker speech, Queue<string> dialogMessages) {
        cts?.Cancel();
        cts = new CancellationTokenSource();

        currentSpeech = speech;
        remainingMessages = new Queue<string>(dialogMessages);

        UpdateCharacterInfo(speech.SpeechData);

        commandManager.Pause();

        OpenDialoguePanel();

        DisplayNextMessage().Forget();
    }

    public async UniTask DisplayChoices(List<string> choices, CancellationToken ct) {
        choicePanel.SetActive(true);

        // Создать кнопки для каждого варианта
        foreach (string choice in choices) {
            Button button = Instantiate(choiceButtonPrefab, choicePanel.transform);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice;
            // Button choices settings
        }

        // Await for player input
    }

    // Event Trigger use
    public void HandleInput() {
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
            if (continueIndicator != null) {
                continueIndicator.SetActive(false);
            }
        } else {
            // No more messages, close the dialogue
            CloseDialoguePanel();
        }
    }

    private async UniTask TypeText(string text, CancellationToken ct) {
        dialogueText.text = "";
        isTyping = true;
        currentMessage = text;

        foreach (char letter in text) {
            if (ct.IsCancellationRequested)
                break;

            dialogueText.text += letter;
            if (audioManager != null && currentSpeech != null && currentSpeech.TryGetSpeechSound(out AudioClip clip)) {
                audioManager.PlaySound(clip);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(letterDelay / currentSpeech.SpeechData.typingSpeed), cancellationToken: ct);
        }
        if (continueIndicator != null) {
            continueIndicator.SetActive(true);
        }

        currentMessage = null;
        isTyping = false;
    }

    private async UniTask CompleteCurrentText() {
        if (isTyping) {
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

    private void SetupSkipButton() {
        skipButton.onClick.AddListener(() => {
            // Пропустить весь диалог
            remainingMessages.Clear();
            CloseDialoguePanel();
        });
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

        // Возобновляем выполнение команд
        commandManager.Resume();
    }

    public bool IsDialogueActive() {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }

    public void ForceCloseDialogue() {
        CloseDialoguePanel();
    }
}