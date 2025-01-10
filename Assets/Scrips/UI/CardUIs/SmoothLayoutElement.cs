using UnityEngine;

public class SmoothLayoutElement : MonoBehaviour {
    private RectTransform targetLayoutElement; // ������� �������, �� ����� ����� ������� �����������
    public float followSpeed = 10f; // �������� ����������
    private const float Threshold = 0.01f;

    private RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(RectTransform layoutElement) {
        targetLayoutElement = layoutElement;
    }

    private void Update() {
        if (targetLayoutElement == null || IsAproached()) return;

        // ������ ����������
        rectTransform.position = Vector3.Lerp(rectTransform.position, targetLayoutElement.position, followSpeed * Time.deltaTime);

        // ������ ���� ������
        rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, targetLayoutElement.sizeDelta, followSpeed * Time.deltaTime);
    }

    private bool IsAproached() {
        float distanse = Vector3.Distance(rectTransform.position, targetLayoutElement.position);
        float sizeDifference = Vector2.Distance(rectTransform.sizeDelta, targetLayoutElement.sizeDelta);
        return distanse < Threshold &&
           sizeDifference < Threshold;
    }

}
