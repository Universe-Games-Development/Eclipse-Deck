using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ZoneView : AreaView {
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] public Transform _creaturesContainer;
    [SerializeField] private Renderer zoneRenderer;
    [SerializeField] private Color unAssignedColor;
    [SerializeField] private Button removeCreatureButton;

    
    [SerializeField] public LayoutSettings settings;
    [Inject] IComponentPool<CreatureView> creaturePool;
    [SerializeField] private List<CreatureView> creatureViews = new();

    private ILayout3DHandler _layout;

    public event Action OnRemoveDebugRequest;

    [SerializeField] bool doTestUpdate = false;
    [SerializeField] float updateDelay = 1f;
    private float updateTimer;
    public event Action OnUpdateRequest;

    protected override void Awake() {
        base.Awake();

        _layout = new Grid3DLayout(settings);

        SetupUI();
    }

    private void Update() {
        if (doTestUpdate) {
            updateTimer += Time.deltaTime;
            if (updateTimer > updateDelay) {
                OnUpdateRequest?.Invoke();
                updateTimer = 0f;
            }
        }
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

        creatureViews.Add(creatureView);
        creatureView.transform.SetParent(_creaturesContainer);
    }

    public void RemoveCreatureView(CreatureView creatureView) {
        if (creatureView == null) return;

        creatureViews.Remove(creatureView);
        creaturePool.Release(creatureView);
    }

    public async UniTask RearrangeCreatures(float duration = 0.5f) {
        var tasks = new List<UniTask>();

        ItemLayoutInfo[] items = new ItemLayoutInfo[creatureViews.Count];

        for (int i = 0; i < creatureViews.Count; i++) {
            items[i] = new ItemLayoutInfo($"{creatureViews[i].name}_i", settings.itemSizes);
        }

        LayoutResult layoutResult = _layout.Calculate(items);
        LayoutPoint[] positions = layoutResult.Points;

        for (int i = 0; i < creatureViews.Count; i++) {
            if (i < positions.Length) {
                CreatureView view = creatureViews[i];
                LayoutPoint point = positions[i];
                Vector3 localPosition = _creaturesContainer.TransformPoint(point.Position);

                Tweener moveTween = view.transform.DOMove(localPosition, duration)
                                    .SetEase(Ease.OutQuad)
                                    .SetLink(view.gameObject);

                tasks.Add(view.DoTweener(moveTween));
            }
        }

        await UniTask.WhenAll(tasks);
    }
    
    public void UpdateSummonedCount(int count) {
        if (text != null) {
            text.text = $"Units: {count}";
        }
    }
    #endregion

    public void ChangeColor(Color newColor) {
        Color color = newColor != null ? newColor : unAssignedColor;
        zoneRenderer.material.color = color;
    }

    public Vector3 CalculateSize(int maxCreatures) {
        ItemLayoutInfo[] items = new ItemLayoutInfo[maxCreatures];

        for (int i = 0; i < maxCreatures; i++) {
            items[i] = new ItemLayoutInfo($"{i}", settings.itemSizes);
        }

        LayoutResult layoutResult = _layout.Calculate(items);
        LayoutMetadata metadata = layoutResult.Metadata;

        float totalLength = metadata.TotalLength;
        float totalWidth = metadata.TotalWidth;
        return new Vector3(totalWidth, 1f, totalLength);
    }
}

