using UnityEngine;
using UnityEngine.Pool;

namespace BasicAudioManager {
    public class SfxManager : MonoBehaviour {
        [SerializeField] private AudioSource _mainSfxSource;
        [SerializeField] private TemporaryAudioSource _audioSourcePrefab;
        [SerializeField] private int _poolDefaultCapacity = 10;
        [SerializeField] private int _poolMaxSize = 30;

        private ObjectPool<TemporaryAudioSource> _audioSourcePool;
        private Transform _poolContainer;
        private bool _isEnabled = true;

        public bool IsEnabled {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        private void Awake() {
            InitializeMainSource();
            InitializePool();
        }

        private void InitializeMainSource() {
            if (_mainSfxSource == null) {
                GameObject sfxObj = new GameObject("MainSFXSource");
                sfxObj.transform.SetParent(transform);
                _mainSfxSource = sfxObj.AddComponent<AudioSource>();
                _mainSfxSource.loop = false;
                _mainSfxSource.playOnAwake = false;
            }
        }

        private void InitializePool() {
            _poolContainer = new GameObject("SFXPool").transform;
            _poolContainer.SetParent(transform);

            _audioSourcePool = new ObjectPool<TemporaryAudioSource>(
                createFunc: CreatePooledSource,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                defaultCapacity: _poolDefaultCapacity,
                maxSize: _poolMaxSize
            );
        }

        private TemporaryAudioSource CreatePooledSource() {
            TemporaryAudioSource source = Instantiate(_audioSourcePrefab, _poolContainer);
            source.Initialize(ReturnToPool);
            return source;
        }

        private void OnGetFromPool(TemporaryAudioSource source) => source.gameObject.SetActive(true);

        private void OnReleaseToPool(TemporaryAudioSource source) {
            source.Stop();
            source.SetClip(null);
            source.transform.SetParent(_poolContainer);
            source.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(TemporaryAudioSource source) {
            if (source != null) Destroy(source.gameObject);
        }

        private void ReturnToPool(TemporaryAudioSource source) {
            if (_audioSourcePool != null && source != null) {
                _audioSourcePool.Release(source);
            }
        }

        public void PlaySound(AudioClip clip, float volume = 1.0f) {
            if (!_isEnabled || clip == null || _mainSfxSource == null) return;
            _mainSfxSource.PlayOneShot(clip, volume);
        }

        public TemporaryAudioSource PlaySoundAtPosition(
            AudioClip clip,
            Vector3 position,
            float volume = 1.0f,
            float spatialBlend = 1.0f) {

            if (!_isEnabled || clip == null || _audioSourcePool == null) return null;

            TemporaryAudioSource source = _audioSourcePool.Get();
            source.SetupSource(clip, position, volume, spatialBlend);
            source.Play();
            return source;
        }

        private void OnDestroy() {
            _audioSourcePool?.Clear();
        }
    }
}