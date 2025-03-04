using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStarter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}

public interface ILoadingOperation {
    public string Desctription { get; }
    Task Load(Action<float> action);
}

public class LoadingScreen : MonoBehaviour {
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _loadingInfo;
    [SerializeField] private Slider _progressFill;
    [SerializeField] private float _barSpeed;

    private float _targetProgress;

    public async UniTask Loading(Queue<ILoadingOperation> operations) {
        _canvas.enabled = true;
        UpdateProgressBar().Forget();
        foreach (var operation in operations) {
            ResetFill();
            _loadingInfo.text = operation.Desctription;
            await operation.Load(OnProgress);
            await WaitForBarFill();
        }

        _canvas.enabled = false;
    }

    private void ResetFill() {
        _progressFill.value = 0;
        _barSpeed = 0;
        _targetProgress = 0;
    }

    private void OnProgress(float progress) {
        _targetProgress = progress;
    }

    private async UniTask WaitForBarFill() {
        while (!Mathf.Approximately(_progressFill.value, _targetProgress)) {
            await UniTask.Yield();
        }
        await UniTask.Delay(TimeSpan.FromSeconds(0.15));
    }


    private async UniTask UpdateProgressBar() {
        while (_canvas.enabled) {
            if (_progressFill.value < _targetProgress) 
                _progressFill.value += Time.deltaTime * _barSpeed;
            await UniTask.Delay(1);
        }
    }
}

public class AlertPopup {
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _closeButton;

    private TaskCompletionSource<bool> _taskCompetition;

    private void Awake() {
        _canvas.enabled = false;
        _okButton.onClick.AddListener(OnACcept);
        _cancelButton.onClick.AddListener(OnCancelled);
        _closeButton.onClick.AddListener(OnCancelled);
    }

    public async UniTask<bool> AwaitForDecision(string text) {
        _text.text = text;
        _canvas.enabled = true;
        _taskCompetition = new TaskCompletionSource<bool>();
        bool result = await _taskCompetition.Task;
        _canvas.enabled = true;
        return result;
    }

    private void OnACcept() {
        _taskCompetition.SetResult(true);
    }

    private void OnCancelled() {
        _taskCompetition.SetResult(false);
    }
}

public class GameResultWindow : MonoBehaviour {
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _restartButton;
    private Action _onRestart;
    private Action _onQuit;


}