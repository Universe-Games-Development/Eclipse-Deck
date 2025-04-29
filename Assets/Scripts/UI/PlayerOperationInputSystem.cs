using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.UI;
using System;
using Zenject;

public interface ITargetingService {
    List<object> FindValidTargets(IRequirement value, IGameContext context);
    UniTask<object> ProcessRequirementAsync(BoardPlayer requestOpponent, IRequirement requirement);
}


public class PlayerOperationInputSystem : MonoBehaviour, ITargetingService {
    [SerializeField] private TMP_Text _instructionText;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private int _timeout = 5;
    [SerializeField] private Button _cancelButton;

    private CardInputHandler _inputHandler;

    private IRequirement _currentRequirement;
    private CancellationTokenSource _buttonCancellation;
    private TimeoutController timeoutController = new ();
    private UniTaskCompletionSource<object> _completionSource;

    private BoardPlayer requestOpponent;

    [Inject]
    public void Construct(CardInputHandler cardInputHandler) {
        _inputHandler = cardInputHandler;
    }

    private void Awake() {
        _cancelButton.onClick.AddListener(Cancel);
        _inputPanel.SetActive(false);
    }

    public async UniTask<object> ProcessRequirementAsync(BoardPlayer requestOpponent, IRequirement requirement) {
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

        // Спочатку перевіряємо наявність IInteractable
        ITargetable interactable = hoveredObject.GetComponent<ITargetable>();
        if (interactable != null) {
            ValidationResult result = interactable.CheckRequirement(_currentRequirement, requestOpponent);
            if (result.IsValid) {
                CompleteRequirement(interactable.GetTargetModel());
            } else {
                _instructionText.text = result.ErrorMessage;
            }
            return;
        }
    }

    private void CompleteRequirement(object result) {
        if (_completionSource != null && !_completionSource.Task.Status.IsCompleted()) {
            _completionSource.TrySetResult(result);
        }
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

    public List<object> FindValidTargets(IRequirement value, IGameContext context) {
        throw new NotImplementedException();
    }
}

public interface ITargetable {
    // Метод для перевірки, чи об'єкт відповідає вимогам
    ValidationResult CheckRequirement(IRequirement requirement, BoardPlayer requestOpponent);

    // Метод для отримання моделі, яка буде передана в результат
    object GetTargetModel();
}

// Конкретна реалізація для різних типів об'єктів
public class CardInteractable : MonoBehaviour, ITargetable {
    [SerializeField] private CardView _view; 
    private CardPresenter _presenter; 

    public void SetPresenter(CardPresenter presenter) {
        _presenter = presenter;
    }

    public ValidationResult CheckRequirement(IRequirement requirement, BoardPlayer requestOpponent) {
        return requirement.Check(requestOpponent, _presenter.Model);
    }

    public object GetTargetModel() {
        return _presenter.Model;
    }
}

public class FieldInteractable : MonoBehaviour, ITargetable {
    [SerializeField] private FieldView _view; 
    private FieldPresenter _presenter; 

    public void SetPresenter(FieldPresenter presenter) {
        _presenter = presenter;
    }

    public ValidationResult CheckRequirement(IRequirement requirement, BoardPlayer requestOpponent) {
        return requirement.Check(requestOpponent, _presenter.Model);
    }

    public object GetTargetModel() {
        return _presenter.Model;
    }
}


public class Opponentnteractable : MonoBehaviour, ITargetable {
    [SerializeField] private OpponentView _view;
    private OpponentPresenter _presenter;

    public void SetPresenter(OpponentPresenter presenter) {
        _presenter = presenter;
    }

    public ValidationResult CheckRequirement(IRequirement requirement, BoardPlayer requestOpponent) {
        return requirement.Check(requestOpponent, _presenter.Model);
    }

    public object GetTargetModel() {
        return _presenter.Model;
    }
}