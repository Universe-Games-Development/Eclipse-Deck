using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CardInputUI : MonoBehaviour, ICardsInputFiller {
    [SerializeField] private RectTransform _root;

    [SerializeField] private Button _cancelButton;
    //[SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _instructionText;

    private CancellationTokenSource _cts;
    TimeoutController timeoutController = new TimeoutController();

    [SerializeField] private RayService RayService;
    [Inject] IInputRequirementRegistry RequirementRegistry;

    public CancellationToken GetCancellationToken() => _cts.Token;
    private void Awake() {
        InitializeButtons();
        _root.gameObject.SetActive(false);
    }

    public void InitializeButtons() {
        _cancelButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.AddListener(Cancel);
    }

    public async UniTask<T> ProcessRequirementAsync<T>(Opponent cardPlayer, CardInputRequirement<T> requirement) where T : MonoBehaviour {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        CancellationToken timeoutToken = timeoutController.Timeout(TimeSpan.FromSeconds(30));

        _root.gameObject.SetActive(true);
        _instructionText.text = requirement.Instruction;

        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeoutToken);

        try {
            T result = await HandleMouseInput(cardPlayer, requirement, linkedTokenSource.Token);
            return result;
        } catch (OperationCanceledException) {
            print("Operation canceled");
            return default;
        } finally {
            linkedTokenSource.Dispose();
            timeoutController.Reset();
            Hide();
        }
    }


    private async UniTask<T> HandleMouseInput<T>(Opponent cardPlayer, CardInputRequirement<T> requirement, CancellationToken token) where T : Component {
        while (!token.IsCancellationRequested) {
            if (Input.GetMouseButtonDown(0)) {
                GameObject rayObject = RayService.GetRayObject();

                if (rayObject != null) {
                    T component = rayObject.GetComponent<T>();
                    if (component != null) {
                        if (requirement.ValidateInput(cardPlayer, component, out string message)) {
                            return component;
                        } else {
                            _instructionText.text = message;
                        }
                    }
                }
            }
            await UniTask.Yield();
        }

        return null;
    }


    private void Cancel() {
        if (_cts != null) {
            _cts.Cancel();
        }
    }

    private void Hide() {
        _instructionText.text = string.Empty;
        _root.gameObject.SetActive(false);
    }

    private void OnDestroy() {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public IInputRequirementRegistry GetRequirementRegistry() {
        return RequirementRegistry;
    }
}

public interface ICardsInputFiller {
    IInputRequirementRegistry GetRequirementRegistry();
    UniTask<T> ProcessRequirementAsync<T>(Opponent cardPlayer, CardInputRequirement<T> requirement) where T : MonoBehaviour;
}
