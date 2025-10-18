using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ZoneView : UnitView, IArea {
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Renderer zoneRenderer;
    [SerializeField] private Color unAssignedColor;
    [SerializeField] private Button removeCreatureButton;

    [Header("Layout")]
    [SerializeField] private ZoneLayoutComponent layoutComponent;

    [Header("Pool")]
    [Inject] private IComponentPool<CreatureView> creaturePool;

    [Header("Visual Manager")]
    [Inject] private IVisualManager visualManager;

    [Header("Animation")]
    [SerializeField] private float appearDuration = 0.5f;
    [SerializeField] private float disappearDuration = 0.5f;
    [SerializeField] private float rearrangeDuration = 0.5f;

    public event Action OnRemoveDebugRequest;

    #region IArea 
    [SerializeField] private AreaBody ownBody;
    public event Action<Vector3> OnSizeChanged;

    public Vector3 Size => ownBody.Size;

    public void Resize(Vector3 newSize) {
        ownBody.Resize(newSize);
        OnSizeChanged?.Invoke(newSize);
    }
    #endregion

    protected void Awake() {
        ValidateReferences();
        SetupLayoutComponent();
        SetupUI();
    }

    private void ValidateReferences() {
        if (layoutComponent == null)
            throw new UnassignedReferenceException(nameof(layoutComponent));
    }

    private void SetupLayoutComponent() {
        layoutComponent.OnLayoutCalculated += HandleLayoutCalculated;
        layoutComponent.OnItemPositioned += HandleCreaturePositioned;
    }

    private void SetupUI() {
        if (zoneRenderer == null) {
            zoneRenderer = GetComponent<Renderer>();
        }

        if (removeCreatureButton != null) {
            removeCreatureButton.onClick.AddListener(() => OnRemoveDebugRequest?.Invoke());
        }
    }

    #region Creatures - Public Synchronous API

    public void SummonCreatureView(CreatureView creatureView) {
        if (creatureView == null) {
            Debug.LogWarning("Trying to add null CreatureView");
            return;
        }

        var summonTask = new SummonCreatureVisualTask(
            creatureView,
            layoutComponent,
            appearDuration
        );
        visualManager.Push(summonTask);
    }

    public void RemoveCreatureView(CreatureView creatureView) {
        if (creatureView == null) {
            Debug.LogWarning("Trying to remove null CreatureView");
            return;
        }

        var removeTask = new RemoveCreatureVisualTask(
            creatureView,
            layoutComponent,
            creaturePool,
            disappearDuration
        );
        visualManager.Push(removeTask);
    }

    public void RearrangeCreatures() {
        var rearrangeTask = new RearrangeCreaturesVisualTask(
            layoutComponent,
            rearrangeDuration
        );
        visualManager.Push(rearrangeTask);
    }

    #endregion

    #region Info

    public void UpdateSummonedCount(int count) {
        if (text != null) {
            text.text = $"Units: {count}";
        }
    }

    public Vector3? GetCreaturePosition(CreatureView creature) {
        return layoutComponent.GetPosition(creature);
    }

    #endregion

    #region Zone

    public void ChangeColor(Color newColor) {
        Color color = newColor != Color.clear ? newColor : unAssignedColor;
        if (zoneRenderer != null) {
            zoneRenderer.material.color = color;
        }
    }

    public Vector3 CalculateRequiredSize(int maxCreatures) {
        return layoutComponent.CalculateRequiredSize(maxCreatures);
    }

    #endregion

    #region Layout Events

    private void HandleLayoutCalculated(LayoutResult result) {
        Debug.Log($"Zone layout: {result.Metadata.TotalItems} creatures, " +
                  $"size: {result.Metadata.TotalWidth:F2}x{result.Metadata.TotalLength:F2}");
    }

    private void HandleCreaturePositioned(CreatureView creature, LayoutPoint point) {
        // Custom логіка при позиціонуванні створіння
    }

    #endregion

    protected void OnDestroy() {
        if (layoutComponent != null) {
            layoutComponent.OnLayoutCalculated -= HandleLayoutCalculated;
            layoutComponent.OnItemPositioned -= HandleCreaturePositioned;
            layoutComponent.ClearItems();
        }

        if (removeCreatureButton != null) {
            removeCreatureButton.onClick.RemoveAllListeners();
        }
    }
}

#region Visual Tasks

public class SummonCreatureVisualTask : VisualTask {
    private readonly CreatureView _creatureView;
    private readonly ZoneLayoutComponent _layout;
    private readonly float _animationDuration;

    public SummonCreatureVisualTask(
        CreatureView creatureView,
        ZoneLayoutComponent layout,
        float animationDuration = 0.5f) {
        _creatureView = creatureView;
        _layout = layout;
        _animationDuration = animationDuration;
    }

    public override async UniTask<bool> Execute() {
        // Активуємо creature
        _creatureView.gameObject.SetActive(true);

        // Додаємо в layout і перераховуємо
        _layout.AddItem(_creatureView, recalculate: true);

        // Анімуємо всіх створінь на нові позиції
        float duration = _animationDuration * TimeModifier;
        await _layout.AnimateAllToLayoutPositions(duration);

        return true;
    }
}

public class RemoveCreatureVisualTask : VisualTask {
    private readonly CreatureView _creatureView;
    private readonly ZoneLayoutComponent _layout;
    private readonly IComponentPool<CreatureView> _creaturePool;
    private readonly float _animationDuration;

    public RemoveCreatureVisualTask(
        CreatureView creatureView,
        ZoneLayoutComponent layout,
        IComponentPool<CreatureView> creaturePool,
        float animationDuration = 0.5f) {
        _creatureView = creatureView;
        _layout = layout;
        _creaturePool = creaturePool;
        _animationDuration = animationDuration;
    }

    public override async UniTask<bool> Execute() {
        _creatureView.gameObject.SetActive(true);
        float duration = _animationDuration * TimeModifier;

        // Анімуємо зникнення створіння
        Sequence fadeSequence = DOTween.Sequence()
            .Join(_creatureView.transform.DOScale(Vector3.zero, duration * 0.5f))
            .Join(_creatureView.transform.DOMoveY(
                _creatureView.transform.position.y - 1f,
                duration * 0.5f
            ));

        await fadeSequence.Play().ToUniTask();

        // Видаляємо з layout
        _layout.RemoveItem(_creatureView, recalculate: true);

        // Анімуємо перерозподіл решти створінь
        await _layout.AnimateAllToLayoutPositions(duration);

        // Повертаємо в pool
        _creaturePool.Release(_creatureView);

        return true;
    }
}

public class RearrangeCreaturesVisualTask : VisualTask {
    private readonly ZoneLayoutComponent _layout;
    private readonly float _animationDuration;

    public RearrangeCreaturesVisualTask(
        ZoneLayoutComponent layout,
        float animationDuration = 0.5f) {
        _layout = layout;
        _animationDuration = animationDuration;
    }

    public override async UniTask<bool> Execute() {
        // Перераховуємо layout
        _layout.RecalculateLayout();

        // Анімуємо всіх на нові позиції
        float duration = _animationDuration * TimeModifier;
        await _layout.AnimateAllToLayoutPositions(duration);

        return true;
    }
}

#endregion