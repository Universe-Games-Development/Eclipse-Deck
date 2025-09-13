using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour {
    [SerializeField] private Renderer cardRenderer;
    private Material _instancedMaterial;
    private int _defaultRenderQueue;

    private void Awake() {
        InitializeMaterials();
    }

    private void Start() {
        DoTest().Forget();
    }

    private void InitializeMaterials() {
        if (cardRenderer != null && cardRenderer.sharedMaterial != null) {
            _instancedMaterial = new Material(cardRenderer.sharedMaterial);
            cardRenderer.material = _instancedMaterial;
            _defaultRenderQueue = _instancedMaterial.renderQueue;
            currentRenderOrder = _defaultRenderQueue;
        }
    }

    public void SetRenderOrder(int order) {
        if (_instancedMaterial != null) {
            _instancedMaterial.renderQueue = order;
        }
    }

    public int tries = 100;
    public int delay = 500;
    public int renderStep = 10;
    public int currentRenderOrder;

    private async UniTask DoTest() {
        // Начинаем с текущего значения и меняем в правильном диапазоне
        int startValue = _defaultRenderQueue;

        for (int i = 0; i < tries; i++) {
            int renderValue = startValue + (i * renderStep);

            // Ограничиваем значения разумными пределами
            renderValue = Mathf.Clamp(renderValue, 1000, 3000);

            SetRenderOrder(renderValue);
            currentRenderOrder = renderValue;
            Debug.Log($"Render Order: {currentRenderOrder}");

            await UniTask.Delay(delay);
        }
    }
}