using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

// Creates CardUIView for 3D cards and render textures for it
public interface ICardTextureRenderer {
    CardUIView Register3DCard(Card3DView card3DView);
    void UnRegister3DCard(Card3DView card3DView);
}

public class Card3DView : CardView {
    public event Action OnInitialized;

    [SerializeField] private SkinnedMeshRenderer cardRenderer;
    [SerializeField] private Card3DAnimator animator;

    private static readonly int CardFrontTextureId = Shader.PropertyToID("_CardFrontTexture");
    private MaterialPropertyBlock _propertyBlock;
    private CardUIView _uiReference;
    private Material _instancedMaterial;

    private int defaultRenderQueue;

    //[Inject] private ICardTextureRenderer cardTextureRenderer;

    public void Initialize(CardUIView cardUIView) {
        _uiReference = cardUIView;
        CardInfo = _uiReference.CardInfo;

        // Create instanced material on initialization
        if (cardRenderer != null && cardRenderer.sharedMaterial != null) {
            _instancedMaterial = new Material(cardRenderer.sharedMaterial);
            cardRenderer.material = _instancedMaterial;
        }

        defaultRenderQueue = cardRenderer.sharedMaterial.renderQueue;

        OnInitialized?.Invoke();
    }

    // Used by ICardTextureRenderer to update the texture
    public void UpdateTexture(Texture2D texture) {
        _propertyBlock ??= new MaterialPropertyBlock();
        // Зчитати поточні property блоку
        cardRenderer.GetPropertyBlock(_propertyBlock);
        // Встановити нову текстуру
        _propertyBlock.SetTexture(CardFrontTextureId, texture);
        // Застосувати блок назад до рендера
        cardRenderer.SetPropertyBlock(_propertyBlock);
    }

    public override void Select() {
        animator.Select();
    }

    public override void Deselect() {
        animator.Deselect();
    }

    public override async UniTask RemoveCardView() {
        isInteractable = false;
        // Play removal animation if needed
        if (animator != null) {
            await animator.PlayRemovalAnimation();
        }
        await base.RemoveCardView();
    }

    public override void SetInteractable(bool value) {
        base.SetInteractable(value);
    }

    private void OnMouseEnter() {
        if (!isInteractable) return;
        OnHoverChanged?.Invoke(this, true);
        Debug.Log("Hover!");
    }

    private void OnMouseExit() {
        if (!isInteractable) return;
        OnHoverChanged?.Invoke(this, false);
        Debug.Log("Hover exit!");
    }

    private void OnMouseUpAsButton() {
        if (!isInteractable) return;
        Debug.Log("Click!");
        //RaiseCardClickedEvent();
    }

    public override void Reset() {
        animator.Reset();
        base.Reset();
    }

    public void SetSortingOrder(int order) {
        if (cardRenderer == null) return;

        // Ensure we have an instanced material
        if (_instancedMaterial == null && cardRenderer.sharedMaterial != null) {
            _instancedMaterial = new Material(cardRenderer.sharedMaterial);
            cardRenderer.material = _instancedMaterial;
        }

        if (_instancedMaterial != null) {
            _instancedMaterial.renderQueue = order;
        }
    }

    public void ResetRenderingOrder() {
        SetSortingOrder(defaultRenderQueue);
    }

    private void OnDestroy() {
        // Clean up the instanced material to prevent memory leaks
        if (_instancedMaterial != null) {
            Destroy(_instancedMaterial);
            _instancedMaterial = null;
        }
    }
}