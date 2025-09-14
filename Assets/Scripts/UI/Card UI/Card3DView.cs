using TMPro;
using UnityEngine;

public class Card3DView : CardView {
    [SerializeField] private Renderer cardRenderer;
    [SerializeField] private Card3DAnimator animator;

    // Кэширование шейдера и материалов
    private static readonly int CardFrontTextureId = Shader.PropertyToID("_CardFrontTexture");
    private MaterialPropertyBlock _propertyBlock;
    private Material _instancedMaterial;
    private int _defaultRenderQueue;

    [Header ("3D info")]
    [SerializeField] TextMeshPro cardName;

    [SerializeField] TextMeshPro costText;
    
    [SerializeField] TextMeshPro healthText;
    [SerializeField] Transform healthIcon;

    [SerializeField] TextMeshPro attackText;
    [SerializeField] Transform attack3DIcon;

    #region Unity Lifecycle

    protected override void Awake() {
        base.Awake();

        _propertyBlock = new MaterialPropertyBlock();
        InitializeMaterials();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
    }

    #endregion

    #region Initialization

    protected override void ValidateComponents() {
        base.ValidateComponents();
        if (cardRenderer == null) {
            Debug.LogError("Card3DView: Renderer not assigned!", this);
        }
    }

    protected override void CleanupResources() {
        base.CleanupResources();

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
        if (!movementComponent.IsMoving) {
            //animator?.Hover(isHovered);
        }
        
    }


    #region 3d Info Update
    public override void UpdateCost(int cost) {
        costText.text = cost.ToString();
    }

    public override void UpdateName(string name) {
        cardName.text = name;
    }

    public override void UpdateAttack(int attack) {
        attackText.text = attack.ToString();
    }

    public override void UpdateHealth(int health) {
        healthText.text = health.ToString();
    }

    public override void ToggleCreatureStats(bool isEnabled) {
       healthIcon.gameObject.SetActive(isEnabled);
        attack3DIcon.gameObject.SetActive(isEnabled);
    }

    public override void UpdatePortait(Sprite portrait) {
        if (_instancedMaterial != null && portrait != null) {
            _instancedMaterial.SetTexture("_Portait", portrait.texture);
        }
    }


    public override void UpdateBackground(Sprite bgImage) {
        //throw new NotImplementedException();
    }

    public override void UpdateRarity(Color rarity) {
        //throw new NotImplementedException();
    }
    #endregion
}