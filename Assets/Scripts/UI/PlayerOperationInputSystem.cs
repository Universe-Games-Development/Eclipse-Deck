using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.UI;
using System;
using Zenject;

public class PlayerOperationInputSystem : MonoBehaviour, IActionFiller {
    [SerializeField] private TMP_Text _instructionText;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private int _timeout = 5;
    [SerializeField] private Button _cancelButton;

    private CardInputHandler _inputHandler;

    private IRequirement _currentRequirement;
    private CancellationTokenSource _buttonCancellation;
    private TimeoutController timeoutController = new TimeoutController();
    private UniTaskCompletionSource<object> _completionSource;
    List<EntityProvider> entityProviders = new();

    [Inject]
    public void Construct(CardInputHandler cardInputHandler) {
        _inputHandler = cardInputHandler;
    }

    private void Awake() {
        _cancelButton.onClick.AddListener(Cancel);
        _inputPanel.SetActive(false);
        entityProviders.Add(new CreatureEntityProvider());
        entityProviders.Add(new FieldEntityProvider());
    }

    public async UniTask<object> ProcessRequirementAsync(IRequirement requirement) {
        SetupUI(requirement);
        _currentRequirement = requirement;

        CancellationToken timeoutToken = timeoutController.Timeout(TimeSpan.FromSeconds(_timeout));
        _completionSource = new UniTaskCompletionSource<object>();
        _buttonCancellation = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            _buttonCancellation.Token,
            timeoutToken
        );

        try {
            _inputHandler.OnLeftClickPerformed += HandleLeftClick;
            return await _completionSource.Task.AttachExternalCancellation(linkedCts.Token);
        } catch (OperationCanceledException) {
            // Операція була скасована (таймаут або кнопка відміни)
            return null;
        } finally {
            _inputHandler.OnLeftClickPerformed -= HandleLeftClick; // Відписка від події
            timeoutController.Reset();
            DisableUI();
        }
    }

    private void HandleLeftClick() {
        if (_completionSource == null || _currentRequirement == null) return;

        GameObject hoveredObject = _inputHandler.hoveredObject;
        if (hoveredObject == null) return;

        // Спроба отримати модель об'єкта
        if (TryGetModelFromGameObject(hoveredObject, out object model)) {
            // Перевірка моделі на відповідність вимогам
            ValidationResult result = _currentRequirement.Check(model);
            if (result.IsValid) {
                CompleteRequirement(model);
            } else {
                _instructionText.text = result.ErrorMessage;
            }
        }
    }

    private void CompleteRequirement(object result) {
        if (_completionSource != null && !_completionSource.Task.Status.IsCompleted()) {
            _completionSource.TrySetResult(result);
        }
    }

    private bool TryGetModelFromGameObject(GameObject hoveredObject, out object model) {
        foreach (var provider in entityProviders) {
            if (provider.TryGetEntity(hoveredObject, out model)) {
                return true;
            }
        }
        model = null;
        return false;
    }

    private void SetupUI(IRequirement requirement) {
        _inputPanel.SetActive(true);
        _instructionText.text = requirement.GetInstruction();

        // Виправлена логіка: активуємо кнопку скасування тільки якщо вибір НЕ є примусовим
        _cancelButton.gameObject.SetActive(!requirement.IsForcedChoice);
    }

    private void DisableUI() {
        _instructionText.text = string.Empty;
        _inputPanel.SetActive(false);
    }

    private void Cancel() {
        if (_buttonCancellation != null && !_buttonCancellation.IsCancellationRequested) {
            _buttonCancellation.Cancel();
        }
    }

    private void OnDestroy() {
        _cancelButton.onClick.RemoveAllListeners();
        _buttonCancellation?.Dispose();
    }
}

public abstract class EntityProvider {
    public abstract bool TryGetEntity(GameObject uiObject, out object entity);
}

public abstract class GenericEntityProvider<TPresenter, TModel> : EntityProvider
    where TPresenter : Component
    where TModel : class {

    protected abstract TModel GetModel(TPresenter presenter);

    public override bool TryGetEntity(GameObject uiObject, out object entity) {
        entity = null;
        var presenter = uiObject.GetComponent<TPresenter>();
        if (presenter != null) {
            var model = GetModel(presenter);
            if (model == null) {
                Debug.LogError($"{typeof(TModel).Name} model not found for {uiObject}");
                return false;
            }
            entity = model;
            return true;
        }
        return false;
    }
}

// Реалізації провайдерів
public class CreatureEntityProvider : GenericEntityProvider<CreaturePresenter, Creature> {
    protected override Creature GetModel(CreaturePresenter presenter) => presenter.Creature;
}

public class FieldEntityProvider : GenericEntityProvider<FieldPresenter, Field> {
    protected override Field GetModel(FieldPresenter presenter) => presenter.Field;
}