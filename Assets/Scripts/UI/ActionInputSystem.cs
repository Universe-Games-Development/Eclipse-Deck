using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.UI;
using System;
using Zenject;

public class ActionInputSystem : MonoBehaviour, IActionFiller {
    [SerializeField] private TMP_Text _instructionText;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private int _timeout = 5;
    [SerializeField] private Button _cancelButton;

    private CancellationTokenSource _cts;
    private TimeoutController timeoutController = new TimeoutController();

    private readonly List<IEntityProvider> _providers = new();
    private CardInputHandler _cardInputHandler;

    [Inject]
    public void Construct(CardInputHandler cardInputHandler) {
        _cardInputHandler = cardInputHandler;
    }

    private void Awake() {
        InitializeButtons();
        _inputPanel.SetActive(false);

        _providers.Add(new CreatureEntityProvider());
        _providers.Add(new FieldEntityProvider());
    }

    public void InitializeButtons() {
        _cancelButton.onClick.AddListener(Cancel);
    }

    public async UniTask<T> ProcessRequirementAsync<T>(Opponent requestingPlayer, IRequirement<T> requirement, CancellationToken externalCt) where T : class {
        CancellationToken timeoutToken = timeoutController.Timeout(TimeSpan.FromSeconds(_timeout));
        _cts = new CancellationTokenSource();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            _cts.Token,
            timeoutToken,
            externalCt
        );
        var token = linkedCts.Token;

        try {
            _instructionText.text = requirement.GetInstruction();
            _inputPanel.SetActive(true);

            return await HandleMouseInput<T>(requestingPlayer, requirement, token);
        } finally {
            _instructionText.text = string.Empty;
            _inputPanel.SetActive(false);
            timeoutController.Reset();
        }
    }

    private async UniTask<T> HandleMouseInput<T>(Opponent requestingPlayer, IRequirement<T> requirement, CancellationToken token) where T : class {
        try {
            while (true) {
                // Очікуємо натискання лівої кнопки миші
                await WaitForLeftClick(token);

                // Отримуємо об'єкт під курсором
                GameObject hitObject = _cardInputHandler.hoveredObject;

                if (hitObject != null && TryGetEntity<T>(hitObject, out T entity)) {
                    if (requirement.IsMet(entity, out string message)) {
                        return entity;
                    } else {
                        _instructionText.text = message;
                    }
                }
            }
        } catch (OperationCanceledException) {
            Debug.Log("Mouse Input operation canceled");
            return default;
        }
    }

    private async UniTask WaitForLeftClick(CancellationToken token) {
        var tcs = new UniTaskCompletionSource<bool>();
        Action onClick = null;

        onClick = () => {
            _cardInputHandler.OnLeftClickPerformed -= onClick;
            tcs.TrySetResult(true);
        };

        _cardInputHandler.OnLeftClickPerformed += onClick;
        using var registration = token.Register(() => {
            _cardInputHandler.OnLeftClickPerformed -= onClick;
            tcs.TrySetCanceled();
        });

        await tcs.Task;
    }

    private bool TryGetEntity<T>(GameObject uiObject, out T entity) where T : class {
        foreach (var provider in _providers) {
            if (provider.TryGetEntity(uiObject, out entity)) {
                return true;
            }
        }

        entity = default;
        return false;
    }

    private void Cancel() {
        if (_cts != null) {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    private void OnDestroy() {
        _cancelButton.onClick.RemoveAllListeners();
        _cts?.Cancel();
        _cts?.Dispose();
    }
}

public interface IEntityProvider {
    bool TryGetEntity<T>(GameObject uiObject, out T entity) where T : class;
}


// Реалізації провайдерів
public class CreatureEntityProvider : IEntityProvider {
    public bool TryGetEntity<T>(GameObject uiObject, out T entity) where T : class {
        entity = null;
        var presenter = uiObject.GetComponent<CreaturePresenter>();
        if (presenter != null && presenter.Model.creatureCard is T card) {
            entity = card;
            return true;
        }
        return false;
    }
}

public class FieldEntityProvider : IEntityProvider {
    public bool TryGetEntity<T>(GameObject uiObject, out T entity) where T : class {
        entity = null;
        var controller = uiObject.GetComponent<FieldPresenter>();
        if (controller != null && controller.Field is T field) {
            entity = field;
            return true;
        }
        return false;
    }
}


public interface IInputVisualizer {
    void HighlightTargets<T>(IRequirement<T> requirement);
    void ClearHighlights();
}