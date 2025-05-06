using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

// Creates CardUIView for 3D cards and render textures for it
public interface ICardTextureRenderer {
    CardUIView Register3DCard(Card3DView card3DView);
    void UnRegister3DCard(Card3DView card3DView);
}


public class Card3DView : CardView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private SkinnedMeshRenderer cardRenderer;
    [SerializeField] private Card3DAnimator animator;
    private static readonly int CardFrontTextureId = Shader.PropertyToID("_CardFrontTexture");
    private MaterialPropertyBlock _propertyBlock;

    private CardUIView _uiReference;

    //[Inject] private ICardTextureRenderer cardTextureRenderer;
    private bool isHovered = false;


    public void Initialize() {
        //_uiReference = cardTextureRenderer.Register3DCard(this);
    }

    public void SetUiReference(CardUIView cardUIView) {
        _uiReference = cardUIView;
        CardInfo = cardUIView.CardInfo;
    }


    public override void InitializeAnimator() {
        if (animator != null) {
            animator.Initialize();
        }
    }

    // Used by ICardTextureRenderer to update the texture
    public void UpdateTexture(Texture2D texture) {
        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();

        // Зчитати поточні property блоку
        cardRenderer.GetPropertyBlock(_propertyBlock);

        // Встановити нову текстуру
        _propertyBlock.SetTexture(CardFrontTextureId, texture);

        // Застосувати блок назад до рендера
        cardRenderer.SetPropertyBlock(_propertyBlock);
    }

    public override void Select() {
        animator?.Select();
    }

    public override void Deselect() {
        animator?.Deselect();
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

        // Update visual state based on interactability
        if (!value && isHovered) {
            isHovered = false;
            animator?.SetHovered(false);
        }
    }
    // Unity Handlers
    public void OnPointerClick(PointerEventData eventData) {
        if (!isInteractable) return;

        animator?.PlayClickAnimation();
        RaiseCardClickedEvent();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!isInteractable) return;

        isHovered = true;
        animator?.SetHovered(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!isInteractable) return;

        isHovered = false;
        animator?.SetHovered(false);
    }

    public override void Reset() {
        animator?.Reset();
        base.Reset();
    }
}


public class Card3DAnimator : MonoBehaviour {
    [Header("Hover Animation")]
    [SerializeField] private float hoverHeight = 0.3f;
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private Ease hoverEase = Ease.OutQuad;

    [Header("Selection Animation")]
    [SerializeField] private float selectedHeight = 0.5f;
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float selectionDuration = 0.3f;
    [SerializeField] private Ease selectionEase = Ease.OutQuad;

    [Header("Click Animation")]
    [SerializeField] private float clickScaleDown = 0.95f;
    [SerializeField] private float clickDuration = 0.1f;
    [SerializeField] private float clickReturnDuration = 0.15f;

    [Header("Removal Animation")]
    [SerializeField] private float removeDuration = 0.5f;
    [SerializeField] private float removeRotation = 90f;
    [SerializeField] private float removeDistance = 5f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private bool isSelected = false;
    private bool isHovered = false;

    private Sequence currentAnimation;

    public void Initialize() {
        // Store original transform values
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    public void SetHovered(bool hovered) {
        isHovered = hovered;

        // Don't animate hover when selected
        if (isSelected) return;

        // Kill previous hover animation if any
        if (currentAnimation != null) {
            currentAnimation.Kill();
            currentAnimation = null;
        }

        // Create hover animation
        currentAnimation = DOTween.Sequence();

        if (hovered) {
            // Hover animation - lift the card up
            currentAnimation.Append(transform.DOLocalMoveY(originalPosition.y + hoverHeight, hoverDuration).SetEase(hoverEase));
        } else {
            // Return to original position
            currentAnimation.Append(transform.DOLocalMoveY(originalPosition.y, hoverDuration).SetEase(hoverEase));
        }
    }

    public void Select() {
        isSelected = true;

        // Kill any ongoing animations
        if (currentAnimation != null) {
            currentAnimation.Kill();
            currentAnimation = null;
        }

        // Create selection animation
        currentAnimation = DOTween.Sequence();

        // Move up and scale
        currentAnimation.Append(transform.DOLocalMoveY(originalPosition.y + selectedHeight, selectionDuration).SetEase(selectionEase));
        currentAnimation.Join(transform.DOScale(originalScale * selectedScale, selectionDuration).SetEase(selectionEase));
    }

    public void Deselect() {
        isSelected = false;

        // Kill any ongoing animations
        if (currentAnimation != null) {
            currentAnimation.Kill();
            currentAnimation = null;
        }

        // Create deselection animation
        currentAnimation = DOTween.Sequence();

        // Return to original position and scale
        currentAnimation.Append(transform.DOLocalMoveY(isHovered ? originalPosition.y + hoverHeight : originalPosition.y, selectionDuration).SetEase(selectionEase));
        currentAnimation.Join(transform.DOScale(originalScale, selectionDuration).SetEase(selectionEase));
    }

    public void PlayClickAnimation() {
        // Quick scale down and up animation
        transform.DOScale(originalScale * clickScaleDown, clickDuration).SetEase(Ease.InQuad)
            .OnComplete(() => {
                transform.DOScale(originalScale, clickReturnDuration).SetEase(Ease.OutQuad);
            });
    }

    public async UniTask PlayRemovalAnimation() {
        // Cancel any ongoing animations
        if (currentAnimation != null) {
            currentAnimation.Kill();
            currentAnimation = null;
        }

        // Create removal animation sequence
        Sequence removalSequence = DOTween.Sequence();

        // Determine direction based on card position
        Vector3 direction = (transform.position.x > 0) ? Vector3.right : Vector3.left;

        // Move away while rotating
        removalSequence.Append(transform.DOLocalRotate(new Vector3(0, 0, removeRotation), removeDuration).SetEase(Ease.InQuad));
        removalSequence.Join(transform.DOLocalMove(transform.localPosition + direction * removeDistance, removeDuration).SetEase(Ease.InQuad));
        removalSequence.Join(transform.DOScale(0, removeDuration).SetEase(Ease.InQuad));

        // Wait for animation to complete
        await removalSequence.AsyncWaitForCompletion();
    }

    public void AnimateStatChange(GameObject statObject, int from, int to) {
        if (statObject == null) return;

        // Flash and scale the stat object
        Sequence statChangeSequence = DOTween.Sequence();

        // Scale up
        statChangeSequence.Append(statObject.transform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutQuad));

        // And back down
        statChangeSequence.Append(statObject.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad));

        // Change color based on whether stat increased or decreased
        if (to > from) {
            // Positive change - green flash
            FlashColor(statObject, Color.green);
        } else if (to < from) {
            // Negative change - red flash
            FlashColor(statObject, Color.red);
        }
    }

    private void FlashColor(GameObject obj, Color flashColor) {
        // Implementation depends on how your stats are rendered
        // This is just a placeholder for the concept
        Debug.Log($"Flashing {obj.name} with color {flashColor}");
    }

    public void Reset() {
        // Kill all animations and reset to original state
        DOTween.Kill(transform);

        if (currentAnimation != null) {
            currentAnimation.Kill();
            currentAnimation = null;
        }

        transform.localPosition = originalPosition;
        transform.localScale = originalScale;
        transform.localRotation = originalRotation;

        isSelected = false;
        isHovered = false;
    }
}