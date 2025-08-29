using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

public class CardHand3DView : CardHandView {
    [Header("Card Orientation")]
    [SerializeField] private Vector3 defaultCardRotation = Vector3.zero;
    [SerializeField] private bool inheritContainerRotation = true;

    [Header("Pool")]
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    private ObjectPool<Card3DView> _cardPool;
    [SerializeField] private int initialPoolSize = 10;
    
    [Inject] private DiContainer diContainer;

    #region Unity Lifecycle

    private void Awake() {
        foreach (Transform child in cardsContainer.transform) {
            Destroy(child.gameObject);
        }

        InitializeCardPool();
    }

    private void OnEnable() {
        // Подписка на события, если необходимо
    }

    private void OnDisable() {
        // Отписка от событий
    }

    

    protected void OnDestroy() {
        _cardPool?.Clear();
    }

    #endregion

    #region Pool Management

    private void InitializeCardPool() {
        if (_cardPool == null) {
            _cardPool = new ObjectPool<Card3DView>(
                createFunc: () => CreateNewView(),
                actionOnGet: card => {
                    card.gameObject.SetActive(true);
                },
                actionOnRelease: card => {
                    card.Reset();
                    card.gameObject.SetActive(false);
                    card.transform.SetParent(cardsContainer);
                },
                actionOnDestroy: card => { if (card && card.gameObject) Destroy(card.gameObject); },
                collectionCheck: false,
                defaultCapacity: initialPoolSize,
                maxSize: 100
            );
        }
    }

    private Card3DView CreateNewView() {
        return diContainer.InstantiatePrefabForComponent<Card3DView>(cardPrefab, cardsContainer);
    }

    #endregion

    #region Bounds Visualization

    

    #endregion

    #region Card Management

    public override CardView BuildCardView() {
        Card3DView card3D = _cardPool.Get();
        return card3D;
    }

    #endregion

}

