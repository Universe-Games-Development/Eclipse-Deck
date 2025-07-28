using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Card3DView : CardView {
    // События
    public event Action<Card3DView, bool> OnCardHoverChanged;
    public event Action OnInitialized;

    // Компоненты рендеринга
    [SerializeField] private SkinnedMeshRenderer cardRenderer;
    [SerializeField] private Card3DAnimator animator;

    // Кэширование шейдера и материалов
    private static readonly int CardFrontTextureId = Shader.PropertyToID("_CardFrontTexture");
    private MaterialPropertyBlock _propertyBlock;
    private CardUIView _uiReference;
    private Material _instancedMaterial;
    private int _defaultRenderQueue;

    #region Unity Lifecycle

    private void Awake() {
        // Создаем MaterialPropertyBlock для эффективного изменения свойств материала
        _propertyBlock = new MaterialPropertyBlock();

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

    private void OnDestroy() {
        // Очищаем экземпляр материала для предотвращения утечек памяти
        if (_instancedMaterial != null) {
            Destroy(_instancedMaterial);
            _instancedMaterial = null;
        }
    }

    #endregion

    #region Initialization

    public void Initialize(CardUIView cardUIView) {
        _uiReference = cardUIView;
        CardInfo = _uiReference.CardInfo;

        // Инициализируем материалы
        InitializeMaterials();

        OnInitialized?.Invoke();
    }

    private void InitializeMaterials() {
        // Создаем экземпляр материала для индивидуального управления
        if (cardRenderer != null && cardRenderer.sharedMaterial != null) {
            _instancedMaterial = new Material(cardRenderer.sharedMaterial);
            cardRenderer.material = _instancedMaterial;
            _defaultRenderQueue = cardRenderer.sharedMaterial.renderQueue;
        }
    }

    public override void Reset() {
        // Сбрасываем все состояния и анимации карты
        if (animator != null) {
            animator.Reset();
        }

        // Сбрасываем порядок рендеринга
        ResetRenderingOrder();

        base.Reset();
    }

    #endregion

    #region Card State Management

    public override void Select() {
        if (animator != null) {
            animator.Select();
        }
    }

    public override void Deselect() {
        if (animator != null) {
            animator.Deselect();
        }
    }

    public override void SetInteractable(bool value) {
        if (!value) {
            // Если карта становится неинтерактивной, а на ней было наведение - сбрасываем
            OnHoverChanged?.Invoke(this, false);
        }

        base.SetInteractable(value);
    }

    #endregion

    #region Mouse Interaction

    private void OnMouseEnter() {
        if (!isInteractable) return;

        OnHoverChanged?.Invoke(this, true);
        OnCardHoverChanged?.Invoke(this, true);

        if (animator != null) {
            animator.Hover(true);
        }
    }

    private void OnMouseExit() {
        if (!isInteractable) return;

        OnHoverChanged?.Invoke(this, false);
        OnCardHoverChanged?.Invoke(this, false);

        if (animator != null) {
            animator.Hover(false);
        }
    }

    private void OnMouseDown() {
        Debug.Log($"Card3DView: Mouse down on card {this}");
    }

    private void OnMouseUpAsButton() {
        if (!isInteractable) return;

        // Вызываем клик по карте
        RaiseCardClickedEvent();
    }
    #endregion

    #region Rendering and Visuals

    // Используется ICardTextureRenderer для обновления текстуры
    public void UpdateTexture(Texture2D texture) {
        if (cardRenderer == null) return;

        // Получаем текущие свойства
        cardRenderer.GetPropertyBlock(_propertyBlock);

        // Устанавливаем новую текстуру
        _propertyBlock.SetTexture(CardFrontTextureId, texture);

        // Применяем изменения к рендереру
        cardRenderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetSortingOrder(int order) {
        _instancedMaterial.renderQueue = order;
    }

    public void ResetRenderingOrder() {
        SetSortingOrder(_defaultRenderQueue);
    }

    #endregion

    #region Card Removal

    public override async UniTask RemoveCardView() {
        // Делаем карту неинтерактивной при удалении
        isInteractable = false;

        // Проигрываем анимацию удаления, если есть
        if (animator != null) {
            await animator.PlayRemovalAnimation();
        }

        // Вызываем базовый метод для завершения удаления
        await base.RemoveCardView();
    }

    #endregion
}
