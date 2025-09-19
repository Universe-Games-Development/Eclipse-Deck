using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "SummonVisualTemplate", menuName = "Operations/Visuals/SummonVisualTemplate")]
public class SummonVisualData : VisualData {
    [Header("Card Transform Effect")]
    public GameObject transformEffectPrefab;
    public float transformDuration = 0.5f;
    public AnimationCurve transformCurve;

    [Header("Creature Materialization")]
    public GameObject materializationEffect;
    public float materializationDelay = 0.2f;

    [Header("Movement")]
    public float aboveAligmentDuration = 1f;
    public Ease moveEase = Ease.OutQuad;
    public Vector3 aligmentHeightOffset = Vector3.up * 3f;
}
