using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "Linear3DHandLayoutSettings", menuName = "CardGame/Layouts/Linear3DHandLayoutSettings")]
public class Linear3DHandLayoutSettings : ScriptableObject {
    [Header("Positioning")]
    [SerializeField] private float _maxHandWidth = 10f;
    [SerializeField] private float _cardThickness = 0.02f;
    [SerializeField] private float _defaultYPosition = 0f;
    [SerializeField] private float _hoverHeight = 0.5f;
    [SerializeField] private float _verticalOffset = 0.1f;

    [Header("Rotation")]
    [SerializeField] private float _maxRotationAngle = 30f;
    [SerializeField] private float _rotationOffset = 5f;

    [Header("Animation")]
    [SerializeField] private float _moveDuration = 0.3f;
    [SerializeField] private float _rotationDuration = 0.2f;
    [SerializeField] private float _hoverMoveDuration = 0.15f;
    [SerializeField] private Ease _moveEase = Ease.OutBack;
    [SerializeField] private Ease _rotationEase = Ease.OutQuad;

    public float MaxHandWidth => _maxHandWidth;
    public float CardThickness => _cardThickness;
    public float DefaultYPosition => _defaultYPosition;
    public float HoverHeight => _hoverHeight;
    public float VerticalOffset => _verticalOffset;
    public float MaxRotationAngle => _maxRotationAngle;
    public float RotationOffset => _rotationOffset;
    public float MoveDuration => _moveDuration;
    public float RotationDuration => _rotationDuration;
    public float HoverMoveDuration => _hoverMoveDuration;
    public Ease MoveEase => _moveEase;
    public Ease RotationEase => _rotationEase;
}