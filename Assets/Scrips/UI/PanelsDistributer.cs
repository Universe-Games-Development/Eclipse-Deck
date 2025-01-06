using UnityEngine;
using UnityEngine.Pool;

public class CreaturePanelDistributer : MonoBehaviour {
    [SerializeField] private GameObject panelPrefab; // ������ �����
    private ObjectPool<GameObject> panelPool; // ��� ��'���� ��� �������

    private Canvas worldSpaceCanvas; // ��������� �� World Space Canvas
    private Transform panelsPool; // ������ �� PanelsPool

    public void Initialize(Canvas canvas) {
        if (canvas == null) {
            Debug.LogError("Canvas is not provided!");
            return;
        }

        worldSpaceCanvas = canvas;

        // ����������, �� ���� PanelsPool, ���� � - ��������� ����
        panelsPool = worldSpaceCanvas.transform.Find("PanelsPool");
        if (panelsPool == null) {
            GameObject panelsPoolObject = new GameObject("PanelsPool");
            panelsPoolObject.transform.SetParent(worldSpaceCanvas.transform);
            panelsPoolObject.transform.localPosition = Vector3.zero;
            panelsPoolObject.transform.localRotation = Quaternion.identity;
            panelsPoolObject.transform.localScale = Vector3.one;
            panelsPool = panelsPoolObject.transform;
        }

        // ����������� ���� �������
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

        // ������ ������ � ����
        return panelPool.Get();
    }

    public void ReleasePanel(GameObject panel) {
        if (panelPool == null) {
            Debug.LogError("Panel pool is not initialized!");
            return;
        }

        // ��������� ������ � ���
        panelPool.Release(panel);
    }
}
