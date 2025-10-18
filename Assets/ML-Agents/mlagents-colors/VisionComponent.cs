using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Простий компонент зору - тільки raycast без логіки кольорів
/// </summary>
public class VisionComponent : MonoBehaviour {
    [SerializeField] private VisionConfig _config;
    [SerializeField] private bool _debugDraw = true;

    private readonly List<Ray> _rays = new List<Ray>(32);
    private readonly List<RaycastHit> _hits = new List<RaycastHit>(32);
    private readonly List<float> _rayLengths = new List<float>(32);
    private readonly RaycastHit[] _hitBuffer = new RaycastHit[1];
    private readonly List<RayHitInfo> _rayHitInfos = new List<RayHitInfo>(32);

    private Vector3 _cachedPosition;
    private Quaternion _cachedRotation;
    private int _expectedRayCount = 0;

    public VisionConfig Config => _config;
    public IReadOnlyList<RayHitInfo> RayHits => _rayHitInfos;
    public int RayCount => _expectedRayCount;

    /// <summary>
    /// Проста структура з інформацією про промінь
    /// Жодної логіки про кольори - тільки що було влучено і на якій відстані
    /// </summary>
    public struct RayHitInfo {
        public bool hasHit;
        public float distance;
        public float normalizedDistance;
        public Vector3 hitPoint;
        public GameObject hitObject;

        public RayHitInfo(bool hit, float dist, float normDist, Vector3 point, GameObject obj) {
            hasHit = hit;
            distance = dist;
            normalizedDistance = normDist;
            hitPoint = point;
            hitObject = obj;
        }
    }

    private void Start() {
        UpdateExpectedRayCount();
    }

    private void Update() {
        if (_config == null) return;

        CacheTransformData();
        GenerateRays();
        PerformRaycasts();
    }

    private void CacheTransformData() {
        _cachedPosition = transform.position;
        _cachedRotation = transform.rotation;
    }

    private void GenerateRays() {
        _rays.Clear();
        _rayLengths.Clear();

        Vector3 forward = _cachedRotation * Vector3.forward;
        Vector3 right = _cachedRotation * Vector3.right;

        AddRaySet(forward, _config.frontRays, _config.frontAngle, _config.frontRayLength);
        AddRaySet(-forward, _config.backRays, _config.backAngle, _config.backRayLength);
        AddRaySet(-right, _config.sideRays, _config.sideAngle, _config.sideRayLength);
        AddRaySet(right, _config.sideRays, _config.sideAngle, _config.sideRayLength);

        UpdateExpectedRayCount();
    }

    private void UpdateExpectedRayCount() {
        if (_config != null) {
            _expectedRayCount = _config.frontRays + _config.backRays + (_config.sideRays * 2);
        }
    }

    private void AddRaySet(Vector3 direction, int count, float angle, float rayLength) {
        if (count <= 0) return;

        if (count == 1) {
            _rays.Add(new Ray(_cachedPosition, direction));
            _rayLengths.Add(rayLength);
            return;
        }

        float step = angle * 2f / (count - 1);
        float startAngle = -angle;

        for (int i = 0; i < count; i++) {
            float currentAngle = startAngle + step * i;
            Vector3 rotatedDir = Quaternion.AngleAxis(currentAngle, Vector3.up) * direction;
            _rays.Add(new Ray(_cachedPosition, rotatedDir));
            _rayLengths.Add(rayLength);
        }
    }

    /// <summary>
    /// Виконує всі raycast-и та збирає базову інформацію
    /// </summary>
    public IReadOnlyList<RayHitInfo> PerformRaycasts() {
        _hits.Clear();
        _rayHitInfos.Clear();

        int rayCount = _rays.Count;
        for (int i = 0; i < rayCount; i++) {
            Ray ray = _rays[i];
            float rayLength = _rayLengths[i];
            bool hasHit = Physics.RaycastNonAlloc(ray, _hitBuffer, rayLength, _config.obstacleMask, _config.triggerInteraction) > 0;

            RayHitInfo hitInfo;

            if (hasHit) {
                RaycastHit hit = _hitBuffer[0];
                _hits.Add(hit);
                hitInfo = new RayHitInfo(true, hit.distance, hit.distance / rayLength, hit.point, hit.collider.gameObject);
            } else {
                hitInfo = new RayHitInfo(false, rayLength, 1f, Vector3.zero, null);
            }

            _rayHitInfos.Add(hitInfo);

            if (_debugDraw) {
                DrawDebugRay(ray, hasHit, hasHit ? _hitBuffer[0].distance : rayLength);
            }
        }
        return RayHits;
    }

    private void DrawDebugRay(Ray ray, bool hasHit, float distance) {
        Color color = hasHit ? _config.debugCloseColor : _config.debugFreeColor;
        Debug.DrawRay(ray.origin, ray.direction * distance, color);
    }

    /// <summary>
    /// Знаходить найближчий об'єкт в полі зору
    /// </summary>
    public bool GetClosestHit(out RayHitInfo closestHit) {
        closestHit = default;
        float minDistance = float.MaxValue;
        bool found = false;

        foreach (var hitInfo in _rayHitInfos) {
            if (hitInfo.hasHit && hitInfo.distance < minDistance) {
                minDistance = hitInfo.distance;
                closestHit = hitInfo;
                found = true;
            }
        }

        return found;
    }

    private void OnValidate() {
        if (_config != null) {
            int estimatedRayCount = _config.frontRays + _config.backRays + (_config.sideRays * 2);
            if (_rays.Capacity < estimatedRayCount) {
                _rays.Capacity = estimatedRayCount;
                _hits.Capacity = estimatedRayCount;
                _rayLengths.Capacity = estimatedRayCount;
                _rayHitInfos.Capacity = estimatedRayCount;
            }
            _expectedRayCount = estimatedRayCount;
        }
    }

    private void OnDrawGizmosSelected() {
        if (_config == null || !Application.isPlaying) return;

        if (GetClosestHit(out var closest)) {
            Gizmos.color = _config.debugCloseColor;
            Gizmos.DrawWireSphere(closest.hitPoint, 0.2f);
        }
    }
}