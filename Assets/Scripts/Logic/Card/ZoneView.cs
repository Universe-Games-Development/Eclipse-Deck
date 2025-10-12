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

    #region Creatures - Synchronous API

    public void SummonCreatureView(CreatureView creatureView, bool doReaarange = true) {
        if (creatureView == null) {
            Debug.LogWarning("Trying to add null CreatureView");
            return;
        }
        //creatureView.transform.localScale = Vector3.zero;
        var addTask = new UniversalVisualTask( () =>
            AnimateCreatureAppear(creatureView),
            $"Creature Add: {creatureView.name}"
        );
        visualManager.Push(addTask);
    }

    /// <summary>
    /// Синхронно видаляє CreatureView і створює візуальну задачу для анімації
    /// </summary>
    public void RemoveCreatureView(CreatureView creatureView) {
        if (creatureView == null) {
            Debug.LogWarning("Trying to remove null CreatureView");
            return;
        }

        // Створюємо візуальну задачу для анімації зникнення
        var removeTask = new UniversalVisualTask(
            AnimateCreatureRemoval(creatureView),
            $"Creature Remove: {creatureView.name}"
        );
        visualManager.Push(removeTask);
    }

    private async UniTask AnimateCreatureAppear(CreatureView creatureView) {
        // Анімація появи (scale 0 -> 1)
        creatureView.gameObject.SetActive(true);

        layoutComponent.AddItem(creatureView);
        await layoutComponent.AnimateAllToLayoutPositions();
    }

    private async UniTask AnimateCreatureRemoval(CreatureView creatureView) {
        // Анімація зникнення
        Sequence fadeSequence = DOTween.Sequence()
            .Join(creatureView.transform.DOScale(Vector3.zero, rearrangeDuration * 0.5f))
            .Join(creatureView.transform.DOMoveY(creatureView.transform.position.y - 1f, rearrangeDuration * 0.5f
        ));

        layoutComponent.RemoveItem(creatureView);
        creaturePool.Release(creatureView);
        await layoutComponent.AnimateAllToLayoutPositions();
    }

    /// <summary>
    /// Синхронно запускає перерозподіл створінь
    /// </summary>
    public void RearrangeCreatures() {
        // Перераховуємо layout синхронно
        layoutComponent.RecalculateLayout();

        // Створюємо візуальну задачу для анімації
        var rearrangeTask = new UniversalVisualTask(
            layoutComponent.AnimateAllToLayoutPositions(rearrangeDuration),
            "Rearrange Creatures"
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