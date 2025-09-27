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
    [SerializeField] private GameObject dialoguePanel;

    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject continueIndicator;

    [SerializeField] private bool dialoguesEnabled = false;

    [Header("Settings")]
    [SerializeField] private float letterDelay = 0.05f;

    private string currentMessage;
    private Speaker currentSpeaker;

    private bool isTyping = false;
    private bool isWaitingForInput = false;

    // Токен для скасування всього діалогу
    private CancellationTokenSource dialogueCts;

    // Токен для скасування тільки поточного виведення тексту
    private CancellationTokenSource typingCts;

    // Токен для очікування введення користувача
    private CancellationTokenSource inputCts;

    [Inject] CommandManager commandManager;
    [Inject] private AudioManager audioManager;

    private void Start() {
        if (dialoguePanel != null) {
            dialoguePanel.SetActive(false);
        }

        if (skipButton != null) {
            skipButton.onClick.AddListener(SkipDialogue);
        }
    }

    public async UniTask StartDialogue(Speaker speaker, Queue<string> messages, CancellationToken dialogueToken = default) {
        if (!dialoguesEnabled) {
            await UniTask.CompletedTask;
            return;
        }

        dialogueCts?.Cancel();
        dialogueCts = new CancellationTokenSource();

        currentSpeaker = speaker;
        Queue<string> remainingMessages = new Queue<string>(messages);

        OpenDialoguePanel();

        try {
            while (remainingMessages.Count > 0 && !dialogueToken.IsCancellationRequested) {
                string message = remainingMessages.Dequeue();

                // Створюємо новий токен для виведення тексту
                typingCts = new CancellationTokenSource();

                // Об'єднуємо токени, щоб скасування діалогу також скасувало виведення тексту
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(dialogueToken, typingCts.Token)) {
                    await TypeText(message, linkedCts.Token);
                }

                if (dialogueToken.IsCancellationRequested) break;

                inputCts = new CancellationTokenSource();

                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(dialogueToken, inputCts.Token)) {
                    isWaitingForInput = true;
                    if (continueIndicator != null) {
                        continueIndicator.SetActive(true);
                    }

                    await WaitForPlayerInput(linkedCts.Token);
                }

                // Приховуємо індикатор продовження
                if (continueIndicator != null) {
                    continueIndicator.SetActive(false);
                }
            }
        } finally {
            CloseDialoguePanel();
        }
    }

    public void HandleInput() {
        if (isTyping) {
            typingCts?.Cancel();
        } else if (isWaitingForInput) {
            inputCts?.Cancel();
            isWaitingForInput = false;
        }
    }

    public void SkipDialogue() {
        dialogueCts?.Cancel();
    }

    private async UniTask WaitForPlayerInput(CancellationToken ct) {
        try {
            // Чекаємо, поки токен не буде скасовано
            await UniTask.WaitUntilCanceled(ct);
        } catch (OperationCanceledException) {
            // Обробка скасування
        } finally {
            isWaitingForInput = false;
        }
    }

    private async UniTask TypeText(string text, CancellationToken ct) {
        dialogueText.text = "";
        isTyping = true;
        currentMessage = text;

        try {
            foreach (char letter in text) {
                // Перевіряємо скасування
                if (ct.IsCancellationRequested) break;

                dialogueText.text += letter;

                // Відтворюємо звук, якщо є
                if (audioManager != null && currentSpeaker != null && currentSpeaker.TryGetSpeechSound(out AudioClip clip)) {
                    audioManager.PlaySound(clip);
                }

                // Затримка між буквами
                await UniTask.Delay(
                    TimeSpan.FromSeconds(letterDelay / currentSpeaker.SpeechData.typingSpeed),
                    cancellationToken: ct
                );
            }
        } catch (OperationCanceledException) {
            // Якщо виведення тексту скасовано, показуємо весь текст одразу
            dialogueText.text = text;
        } finally {
            isTyping = false;
            currentMessage = null;
        }
    }

    public void UpdateCharacterInfo(OpponentData opponentData) {
        characterName.text = opponentData.Name;
        characterSprite.sprite = opponentData.Sprite;
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

        // Очищаємо всі токени
        dialogueCts?.Cancel();
        typingCts?.Cancel();
        inputCts?.Cancel();

        // Очищаємо стан
        dialogueCts = null;
        typingCts = null;
        inputCts = null;
        currentSpeaker = null;
        isTyping = false;
        isWaitingForInput = false;
    }

    public bool IsDialogueActive() {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }

    public void ForceCloseDialogue() {
        CloseDialoguePanel();
    }
}

//public class DialogueCommand : Command {
//    private readonly Speaker speaker;
//    private readonly Queue<string> dialogMessages;
//    private readonly CancellationTokenSource dialogueCts;
//    private readonly DialogueSystem dialogueSystem;

//    public DialogueCommand(Speaker speaker, Queue<string> dialogMessages, CancellationTokenSource dialogueCts, DialogueSystem dialogueSystem) {
//        this.speaker = speaker;
//        this.dialogMessages = dialogMessages;
//        this.dialogueCts = dialogueCts;
//        this.dialogueSystem = dialogueSystem;

//        // Встановлюємо високий пріоритет для діалогів
//        Priority = CommandPriority.High;
//    }

//    public override async UniTask Execute() {
//        // Встановлюємо дані про персонажа
//        dialogueSystem.UpdateCharacterInfo(speaker.Opponent.Data);

//        // Починаємо показ повідомлень
//        await dialogueSystem.StartDialogue(dialogMessages, dialogueCts.Token);

//        // Повертаємо завершене завдання
//        return;
//    }

//    public override UniTask Undo() {
//        // Скасовуємо діалог
//        dialogueSystem.ForceCloseDialogue();
//        return UniTask.CompletedTask;
//    }
//}
