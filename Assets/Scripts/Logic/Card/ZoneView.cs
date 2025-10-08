using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ZoneView : InteractableView, IArea {
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Renderer zoneRenderer;
    [SerializeField] private Color unAssignedColor;
    [SerializeField] private Button removeCreatureButton;

    [Header("Layout")]
    [SerializeField] private ZoneLayoutComponent layoutComponent;
    [SerializeField] private Transform creaturesContainer;

    [Header("Pool")]
    [Inject] private IComponentPool<CreatureView> creaturePool;

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

    protected override void Awake() {
        base.Awake();
        ValidateReferences();
        SetupLayoutComponent();
        SetupUI();
    }

    private void ValidateReferences() {
        if (layoutComponent == null)
            throw new UnassignedReferenceException(nameof(layoutComponent));
        if (creaturesContainer == null)
            throw new UnassignedReferenceException(nameof(creaturesContainer));
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

    

    #region Creatures

    public void AddCreatureView(CreatureView creatureView) {
        if (creatureView == null) return;

        creatureView.transform.SetParent(creaturesContainer);
        // Layout component сам встановить parent
        layoutComponent.AddItem(creatureView, recalculate: true);

        // Анімуємо на позицію
        layoutComponent.AnimateToLayoutPosition(creatureView).Forget();
    }

    public void RemoveCreatureView(CreatureView creatureView) {
        if (creatureView == null) return;

        // Видаляємо з layout
        layoutComponent.RemoveItem(creatureView, recalculate: true);

        // Анімуємо решту створінь
        RearrangeCreatures(rearrangeDuration).Forget();

        // Повертаємо в pool
        creaturePool?.Release(creatureView);
    }

    public async UniTask RearrangeCreatures(float duration = 0.5f) {
        await layoutComponent.AnimateAllToLayoutPositions(duration);
    }

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
