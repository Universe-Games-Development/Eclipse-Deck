using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class UnitViewProvider : MonoBehaviour {
    [SerializeField] private UnitView _unitView;

    public event Action OnClicked;
    public event Action OnPointerEnter;
    public event Action OnPointerExit;

    private void Awake() {
        if (_unitView == null) {
            _unitView = GetComponentInParent<UnitView>();
        }

        if (_unitView == null) {
            Debug.LogError($"UnitView not found for provider on {gameObject.name}", this);
        }
    }

    public UnitView GetUnitView() => _unitView;

    // --- Unity UI events ---
    private void OnMouseDown() {
        // �������� ��������������� ��� �������� ����
        OnClicked?.Invoke();
    }

    private void OnMouseEnter() {
        // ��������� ������� �� ��������
        OnPointerEnter?.Invoke();
    }

    private void OnMouseExit() {
        // ������ ������� ��������
        OnPointerExit?.Invoke();
    }
}
