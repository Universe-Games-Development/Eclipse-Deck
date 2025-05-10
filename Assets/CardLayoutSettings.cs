using UnityEngine;

[CreateAssetMenu(fileName = "CardLayoutSettings", menuName = "Card Game/Card Layout Settings")]
public class CardLayoutSettings : ScriptableObject {
    [Header("Common Settings")]
    [Tooltip("Base thickness of cards in 3D space")]
    public float cardThickness = 0.1f;
    [Tooltip("Duration of card movement animation")]
    public float animationDuration = 0.3f;
    [Tooltip("Duration of card rotation animation")]
    public float rotationDuration = 0.2f;
    [Tooltip("Maximum delay between sequential card animations")]
    public float maxAnimationDelay = 0.1f;
    [Tooltip("Random Y offset to make cards look more natural")]
    public float maxCardYOffset = 0.2f;

    [Header("Spacing Settings")]
    [Tooltip("Base spacing between cards")]
    public float baseCardSpacing = 1.2f;
    [Tooltip("Minimum spacing between cards")]
    public float minCardSpacing = 0.8f;
    [Tooltip("Maximum spacing between cards")]
    public float maxCardSpacing = 2.5f;
    [Tooltip("Optimal number of cards for base spacing")]
    public int optimalCardCount = 5;
    [Tooltip("Spacing multiplier for fewer cards than optimal")]
    public float fewCardsSpacingMultiplier = 1.8f;
    [Tooltip("Spacing reduction factor when there are many cards")]
    public float manyCardsSpacingReduction = 0.8f;

    [Header("Curved Layout Settings")]
    [Tooltip("Height of the curve for hand layout")]
    public float cardCurveHeight = 0.5f;
    [Tooltip("Maximum rotation angle for cards")]
    public float cardRotationAngle = 15f;

    [Header("Linear Layout Settings")]
    [Tooltip("Base spacing for linear layout")]
    public float linearBaseSpacing = 1.5f;

    /// <summary>
    /// Calculates appropriate card spacing based on the number of cards in hand
    /// </summary>
    /// <param name="cardCount">Number of cards in hand</param>
    /// <param name="isLinearLayout">Whether using linear layout (vs curved)</param>
    /// <returns>Optimized spacing value</returns>
    public float CalculateCardSpacing(int cardCount, bool isLinearLayout = false) {
        float baseSpacing = isLinearLayout ? linearBaseSpacing : baseCardSpacing;

        if (cardCount <= 0) return baseSpacing;

        // For few cards, increase spacing
        if (cardCount < optimalCardCount) {
            float multiplier = Mathf.Lerp(fewCardsSpacingMultiplier, 1f, cardCount / (float)optimalCardCount);
            return Mathf.Min(baseSpacing * multiplier, maxCardSpacing);
        }
        // For many cards, decrease spacing to avoid spreading too wide
        else if (cardCount > optimalCardCount) {
            float reductionFactor = Mathf.Lerp(1f, manyCardsSpacingReduction,
                Mathf.Min(1f, (cardCount - optimalCardCount) / 10f));
            return Mathf.Max(baseSpacing * reductionFactor, minCardSpacing);
        }

        return baseSpacing;
    }
}