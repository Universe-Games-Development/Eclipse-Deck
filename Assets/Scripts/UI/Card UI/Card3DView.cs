using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Card3DView : CardView {
    // Компоненты рендеринга
    [SerializeField] private SkinnedMeshRenderer cardRenderer;
    //[SerializeField] private Card3DAnimator animator;
    public Action OnInitialized;

    // Кэширование шейдера и материалов
    private static readonly int CardFrontTextureId = Shader.PropertyToID("_CardFrontTexture");
    private MaterialPropertyBlock _propertyBlock;
    private Material _instancedMaterial;
    private int _defaultRenderQueue;


    #region Unity Lifecycle

    protected override void Awake() {
        base.Awake();

        // Создаем MaterialPropertyBlock для эффективного изменения свойств материала
        _propertyBlock = new MaterialPropertyBlock();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
    }

    #endregion

    #region Initialization

    public void SyncWithUICopy(CardUIView cardUIView) {
        CardInfo = cardUIView.CardInfo;
        InitializeMaterials();
        OnInitialized.Invoke();
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
        ResetRenderingOrder();
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

    public void SetSortingOrder(int order) {
        if (_instancedMaterial != null) {
            _instancedMaterial.renderQueue = order;
        }
    }

    public void ResetRenderingOrder() {
        SetSortingOrder(_defaultRenderQueue);
    }

    #endregion

    #region Card Removal

    protected override async UniTask PlayRemovalAnimation() {
        // Проигрываем анимацию удаления, если есть
        //if (animator != null) {
        //    await animator.PlayRemovalAnimation();
        //}
        await UniTask.Yield();
    }

    #endregion
}