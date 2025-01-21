using UnityEngine;

[CreateAssetMenu (fileName = "LevitationData", menuName = "PrefabSettings/Levitation"), ]
public class LevitationData : ScriptableObject
{
    [Header("Start Leviation")]
    public float liftHeight = 1f;
    public float liftDuration = 0.5f;

    [Header("Continuous Leviation")]
    public float levitationSpeed = 1f;
    public float levitationRange = 0.2f;

    [Header("Stop Leviation")]
    public float dropDuration = 0.5f;

    [Header("Spawn fly")]
    public float spawnHeight = 10f;
    public float spawnDuration = 0.5f;

    [Header("Fly away")]
    public float flyHeight = 10f;
    public float flyAwayDuration = 0.5f;
}
