using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Collider))]
public class Card3DView : CardView {
    [Inject] private CardTextureRenderer textureRenderer;
    [SerializeField] private int hoverRenderOrderBoost = 80;
    [SerializeField] private SkinnedMeshRenderer cardRenderer;
    [SerializeField] private Card3DAnimator animator;
    public Action OnInitialized;

    // Кэширование шейдера и материалов
    private static readonly int CardFrontTextureId = Shader.PropertyToID("_CardFrontTexture");
    private MaterialPropertyBlock _propertyBlock;
    private Material _instancedMaterial;
    private int _defaultRenderQueue;

    public CardUIView card2DView;

    public bool IsInitialized { get; private set; }

    #region Unity Lifecycle

    protected override void Awake() {
        base.Awake();

        // Создаем MaterialPropertyBlock для эффективного изменения свойств материала
        _propertyBlock = new MaterialPropertyBlock();
        card2DView = textureRenderer.Register3DCard(this);
        SyncWithUICopy(card2DView);
    }

    private void Start() {
        
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        textureRenderer.UnRegister3DCard(this);
    }

    #endregion

    #region Initialization

    private void SyncWithUICopy(CardUIView cardUIView) {
        card2DView = cardUIView;
        uiInfo = card2DView.uiInfo;

        InitializeMaterials();
        IsInitialized = true;
        OnInitialized?.Invoke();
    }

    protected override void ValidateComponents() {
        base.ValidateComponents();

        // Проверяем наличие компонентов рендеринга
        if (cardRenderer == null) {
            Debug.LogError("Card3DView: SkinnedMeshRenderer not assigned!", this);
        }

        // Проверяем наличие коллайдера для взаимодействия
        Collider col = GetComponent<Collider>();
        if (col == null) {
            Debug.LogWarning("Card3DView: No Collider component found! Adding BoxCollider.", this);
            gameObject.AddComponent<BoxCollider>();
        }
    }

    protected override void CleanupResources() {
        base.CleanupResources();

        // Очищаем экземпляр материала для предотвращения утечек памяти
        if (_instancedMaterial != null) {
            Destroy(_instancedMaterial);
            _instancedMaterial = null;
        }
    }

    private void InitializeMaterials() {
        // Создаем экземпляр материала для индивидуального управления
        if (cardRenderer != null && cardRenderer.sharedMaterial != null) {
            _instancedMaterial = new Material(cardRenderer.sharedMaterial);
            cardRenderer.material = _instancedMaterial;
            _defaultRenderQueue = cardRenderer.sharedMaterial.renderQueue;
        }
    }

    #endregion

    #region State Management

    protected override void ResetVisualState() {
        base.ResetVisualState();

        // Сбрасываем анимации
        //if (animator != null) {
        //    animator.Reset();
        //}

        // Сбрасываем порядок рендеринга
        ResetRenderOrder();
    }
    #endregion

    #region Mouse Interaction

    private void OnMouseEnter() {
        HandleMouseEnter();
    }

    private void OnMouseExit() {
        HandleMouseExit();
    }

    private void OnMouseDown() {
        HandleMouseDown();
    }

    #endregion

    #region Rendering and Visuals

    // Используется ICardTextureRenderer для обновления текстуры
    public void UpdateTexture(Texture2D texture) {
        if (cardRenderer == null) return;

        // Получаем текущие свойства
        cardRenderer.GetPropertyBlock(_propertyBlock);

        // Устанавливаем новую тектуру
        _propertyBlock.SetTexture(CardFrontTextureId, texture);

        // Применяем изменения к рендереру
        cardRenderer.SetPropertyBlock(_propertyBlock);
    }

    #endregion

    public override void SetRenderOrder(int order) {
        if (_instancedMaterial != null) {
            _instancedMaterial.renderQueue = order;
            //Debug.Log($"{gameObject.name}: Render queue: {order}");
        }
    }

    public override void ModifyRenderOrder(int modifyValue) {
        if (TryGetCurrentRenderOrder(out int currentOrder)) {
            SetRenderOrder(currentOrder + modifyValue);
        }
    }

    public override void ResetRenderOrder() {
        SetRenderOrder(_defaultRenderQueue);
    }

    private bool TryGetCurrentRenderOrder(out int order) {
        order = _instancedMaterial?.renderQueue ?? 0;
        return _instancedMaterial != null;
    }

    public override void SetHoverState(bool isHovered) {
        ModifyRenderOrder(isHovered ? hoverRenderOrderBoost : -hoverRenderOrderBoost);

        animator?.Hover(isHovered);
    }
}