using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.UI;
using System;

public interface IEntityProvider {
    bool TryGetEntity<T>(GameObject uiObject, out T entity) where T : class;
}


// Ðואכ³חאצ³¿ ןנמגאיהונ³ג
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
        var controller = uiObject.GetComponent<FieldController>();
        if (controller != null && controller.LinkedField is T field) {
            entity = field;
            return true;
        }
        return false;
    }
}


public class ActionInputSystem : MonoBehaviour, IActionFiller {
    [SerializeField] private TMP_Text _instructionText;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private int _timeout = 5;
    [SerializeField] private Button _cancelButton;

    private CancellationTokenSource _cts;
    TimeoutController timeoutController = new TimeoutController();

    [SerializeField] private RaycastService RayService;

    private readonly List<IEntityProvider> _providers = new();

    private void Awake() {
        if (RayService == null) {
            Debug.LogError("RayService is not assigned in AbilityInputSystem!");
        }

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
            _instructionText.text = (requirement.GetInstruction());
            _inputPanel.SetActive(true);

            return await HandleMouseInput(requestingPlayer, requirement, token);
        } finally {
            _instructionText.text = string.Empty;
            _inputPanel.SetActive(false);
            timeoutController.Reset();
        }
    }

    private async UniTask<T> HandleMouseInput<T>(Opponent requestingPlayer, IRequirement<T> requirement, CancellationToken token) where T : class {
        try {
            while (!token.IsCancellationRequested) {
                if (Input.GetMouseButtonDown(0)) {
                    GameObject hitObject = RayService.GetRayObject();

                    if (hitObject != null && TryGetEntity<T>(hitObject, out T entity)) {
                        if (requirement.IsMet(entity, out string message)) {
                            return entity;
                        } else {
                            _instructionText.text = message;
                        }
                    }
                }
                await UniTask.DelayFrame(1, cancellationToken: token);
            }
        } catch (OperationCanceledException) {
            Debug.Log("Mouse Input operation canceled");
        }
        return default;
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

public interface IInputVisualizer {
    void HighlightTargets<T>(IRequirement<T> requirement);
    void ClearHighlights();
}