using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ZoneView : AreaView {
    [SerializeField] private TextMeshPro text;
    [SerializeField] private Transform _creaturesContainer;
    [SerializeField] private Renderer zoneRenderer;
    [SerializeField] private Color unAssignedColor;
    [SerializeField] private Button removeCreatureButton;

    private ILayout3DHandler _layout;
    [SerializeField] private LayoutSettings settings;

    // НОВЕ: Список істот в зоні на рівні View
    private readonly List<CreatureView> _creatureViews = new();
    public IReadOnlyList<CreatureView> CreatureViews => _creatureViews;
    [Inject] IComponentPool<CreatureView> creaturePool;

    public event Action OnRemoveDebugRequest;
    public Vector3 additionalOffset = new Vector3(2, 0, 3);

    [SerializeField] bool doTestUpdate = false;
    [SerializeField] float updateDelay = 1f;
    private float updateTimer;

    protected override void Awake() {
        base.Awake();
        InitializeLayout();
        SetupUI();
    }

    private void Update() {
        if (doTestUpdate) {
            updateTimer += Time.deltaTime;
            if (updateTimer > updateDelay) {
                CalculateSize(CreatureViews.Count);
                updateTimer = 0f;
            }
        }
    }

    private void InitializeLayout() {
        if (settings == null) {
            Debug.LogWarning("Settings layout null for ", gameObject);
            return;
        }
        _layout = new Linear3DLayout(settings);
    }

    private void SetupUI() {
        if (zoneRenderer == null) {
            zoneRenderer = GetComponent<Renderer>();
        }

        if (removeCreatureButton != null) {
            removeCreatureButton.onClick.AddListener(() => OnRemoveDebugRequest?.Invoke());
        }
    }

    #region Cereatures
    public void AddCreatureView(CreatureView creatureView) {
        if (creatureView == null) return;

        creatureView.transform.SetParent(_creaturesContainer);
        _creatureViews.Add(creatureView);

        UpdateVisualState();
    }

    public void RemoveCreatureView(CreatureView creatureView) {
        if (creatureView == null) return;

        creaturePool.Release(creatureView);
        _creatureViews.Remove(creatureView);
        UpdateVisualState();
    }

    public async UniTask RearrangeCreatures(float duration = 0.5f) {
        var positions = GetCreaturePoints(_creatureViews.Count);
        var tasks = new List<UniTask>();

        for (int i = 0; i < _creatureViews.Count; i++) {
            if (i < positions.Count) {
                CreatureView view = _creatureViews[i];
                LayoutPoint point = positions[i];

                Tweener moveTween = view.transform.DOMove(point.position, duration)
                                    .SetEase(Ease.OutQuad)
                                    .SetLink(view.gameObject);

                tasks.Add(view.DoTweener(moveTween));
            }
        }

        await UniTask.WhenAll(tasks);
    }
    
    private void UpdateVisualState() {
        UpdateSummonedCount(_creatureViews.Count);
    }

    public void UpdateSummonedCount(int count) {
        if (text != null) {
            text.text = $"Units: {count}";
        }
    }
    
    public List<LayoutPoint> GetCreaturePoints(int count) {
        LayoutResult layoutResult = _layout.CalculateLayout(count);
        var transformedPoints = new List<LayoutPoint>();

        foreach (var point in layoutResult.Points) {
            Vector3 worldPosition = _creaturesContainer.TransformPoint(point.position);
            transformedPoints.Add(new LayoutPoint(worldPosition, point.rotation, point.orderIndex, point.rowIndex, point.columnIndex));
        }

        return transformedPoints;
    }
    #endregion

    public Vector3 CalculateSize(int creaturesCapacity) {
        float areaWidth = settings.ItemWidth;
        float areaLength = settings.ItemLength;

        var layoutResult = _layout.CalculateLayout(creaturesCapacity);
        LayoutMetadata metadata = layoutResult.Metadata;

        float totalLength = metadata.TotalLength;
        float totalWidth = metadata.TotalWidth;

        Vector3 localScale = transform.localScale;
        Vector3 newScale = new Vector3(totalWidth, localScale.y, totalLength) + additionalOffset;
       
        return newScale;
    }

    public Vector3 GetSize() => transform.localScale;

    public void ChangeColor(Color newColor) {
        Color color = newColor != null ? newColor : unAssignedColor;
        zoneRenderer.material.color = color;
    }
}