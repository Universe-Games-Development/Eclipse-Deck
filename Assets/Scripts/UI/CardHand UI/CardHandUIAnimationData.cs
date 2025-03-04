using UnityEngine;

[CreateAssetMenu(fileName = "CardHandAnimationData", menuName = "PrefabSettings/CardHand"),]
public class CardHandUIAnimationData : ScriptableObject {
    [Header("Start Leviation")]
    public float liftHeight = 1f;
    public float liftDuration = 0.5f;

    [Header("Shake")]
    public float shakeDuration = 0.3f;
    public float shakeStrength = 0.3f;
    [Range (0, 10)]
    public int shakeVibration = 3;
    [Range(0, 90)]
    public float shakeRandomness;
}