using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CardHand3DView : CardHandView {
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private CardLayoutSettings layoutSettings;

    [Inject] private CardTextureRenderer cardTextureRenderer;

    private ICardLayoutStrategy _layoutStrategy;
    private Transform _hoveredCardTransform = null;

    [SerializeField] bool debugMode = false;

    [Inject]
    public void Initialize(ICardLayoutStrategy layoutStrategy = null) {
        // ���������, ���� �� ��������� � ����������
        if (layoutSettings == null) {
            Debug.LogWarning("No CardLayoutSettings assigned! Using default values.", this);
        }

        // ���� ��������� �� �������� ����� Zenject, ������� ��������� �� ���������
        if (layoutStrategy == null) {
            // ���������� ��������� �� ����������
            _layoutStrategy = new CurvedHandLayoutStrategy(layoutSettings);
        } else {
            _layoutStrategy = layoutStrategy;
            // �������� ��������� � ���������, ���� ��� ���� �������� ����� Zenject
            _layoutStrategy.SetSettings(layoutSettings);
        }
    }

    public void Update() {
        if (debugMode)
        UpdateCardPositions();
    }

    public void SetLayoutStrategy(ICardLayoutStrategy layoutStrategy) {
        if (layoutStrategy == null) return;

        _layoutStrategy = layoutStrategy;
        // �������� ��������� �� ���������� � ����� ���������
        _layoutStrategy.SetSettings(layoutSettings);
        // ��������� ������������ ���� � ����� ����������
        UpdateCardPositions();
    }

    public void SetLayoutSettings(CardLayoutSettings settings) {
        if (settings == null) return;

        this.layoutSettings = settings;
        // ��������� ��������� � ������� ���������
        if (_layoutStrategy != null) {
            _layoutStrategy.SetSettings(settings);
            UpdateCardPositions();
        }
    }

    public override CardView BuildCardView(string id) {
        if (cardPrefab == null || cardsContainer == null) {
            Debug.LogError("CardPrefab or CardsContainer not set!", this);
            return null;
        }

        Card3DView card3D = Instantiate(cardPrefab, cardsContainer);
        card3D.OnHoverChanged += HandleCardHover;
        cardTextureRenderer.Register3DCard(card3D);

        return card3D;
    }

    private void HandleCardHover(CardView cardView, bool isHovered) {
        if (_layoutStrategy == null) return;

        // ���� ��� ���� ���������� �����, ������� � ��� hover
        if (_hoveredCardTransform != null && !isHovered && _hoveredCardTransform != cardView.transform) {
            _layoutStrategy.SetCardHovered(_hoveredCardTransform, false).Forget();
        }

        // ������������� ����� ���������� �����
        _hoveredCardTransform = isHovered ? cardView.transform : null;

        // ������������� hover ��������� ��� ������� �����
        _layoutStrategy.SetCardHovered(cardView.transform, isHovered).Forget();
    }

    public override void UpdateCardPositions() {
        if (_cardViews.Count == 0) return;

        // �������� ������ Transform ����������� ���� ����
        Transform[] cardTransforms = new Transform[_cardViews.Count];
        int index = 0;
        foreach (var card in _cardViews.Values) {
            if (card != null) {
                cardTransforms[index] = card.transform;
                index++;
            }
        }

        // ��������� ������� ��������� ������������
        _layoutStrategy.LayoutCards(this, cardTransforms).Forget();
    }

    public override void HandleCardViewRemoval(CardView cardView) {
        // ������� ���������� ������� ��������� ����� ���������
        if (cardView is Card3DView card3D) {
            card3D.OnHoverChanged -= HandleCardHover;
        }

        // ���� ��������� ����� ���� ��� ����������, ���������� ���������
        if (_hoveredCardTransform == cardView.transform) {
            _hoveredCardTransform = null;
        }

        // �������� ������� �����
        base.HandleCardViewRemoval(cardView);
    }

    public override void Cleanup() {
        // ������� ��� ����������� �������
        foreach (var cardView in _cardViews.Values) {
            if (cardView is Card3DView card3D) {
                card3D.OnHoverChanged -= HandleCardHover;
            }
        }

        _hoveredCardTransform = null;
        base.Cleanup();
    }
}
