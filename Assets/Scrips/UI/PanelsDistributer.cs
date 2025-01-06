using UnityEngine;
using UnityEngine.Pool;

public class CreaturePanelDistributer : MonoBehaviour {
    [SerializeField] private GameObject panelPrefab; // Префаб панелі
    private ObjectPool<GameObject> panelPool; // Пул об'єктів для панелей

    private Canvas worldSpaceCanvas; // Посилання на World Space Canvas
    private Transform panelsPool; // Ссилка на PanelsPool

    public void Initialize(Canvas canvas) {
        if (canvas == null) {
            Debug.LogError("Canvas is not provided!");
            return;
        }

        worldSpaceCanvas = canvas;

        // Перевіряємо, чи існує PanelsPool, якщо ні - створюємо його
        panelsPool = worldSpaceCanvas.transform.Find("PanelsPool");
        if (panelsPool == null) {
            GameObject panelsPoolObject = new GameObject("PanelsPool");
            panelsPoolObject.transform.SetParent(worldSpaceCanvas.transform);
            panelsPoolObject.transform.localPosition = Vector3.zero;
            panelsPoolObject.transform.localRotation = Quaternion.identity;
            panelsPoolObject.transform.localScale = Vector3.one;
            panelsPool = panelsPoolObject.transform;
        }

        // Ініціалізація пулу панелей
        panelPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(panelPrefab, panelsPool),
            actionOnGet: panel => panel.SetActive(true),
            actionOnRelease: panel => {
                panel.transform.SetParent(panelsPool);
                panel.SetActive(false);
            },
            actionOnDestroy: Destroy
        );
    }

    public GameObject CreatePanel() {
        if (panelPool == null) {
            Debug.LogError("Panel pool is not initialized!");
            return null;
        }

        // Беремо панель з пулу
        return panelPool.Get();
    }

    public void ReleasePanel(GameObject panel) {
        if (panelPool == null) {
            Debug.LogError("Panel pool is not initialized!");
            return;
        }

        // Повертаємо панель в пул
        panelPool.Release(panel);
    }
}
