using UnityEngine;
using Zenject;

public class Opponent : MonoBehaviour {
    public string Name = "Opponent";
    protected CardCollection cardCollection;
    protected Deck deck;
    protected Deck discardDeck;

    protected CardHand hand;

    [SerializeField] private int initHealth;
    [SerializeField] private int maxHealth;

    public Health health;

    private ResourceManager resourceManager;
    private IEventManager eventManager;

    [SerializeField] protected GameBoard tableManager;

    [Inject]
    public void Construct(IEventManager eventManager, ResourceManager resourceManager) {
        this.eventManager = eventManager;
        this.resourceManager = resourceManager;
    }

    protected virtual void Awake() {
        health = new Health(maxHealth, initHealth);
        cardCollection = new CardCollection(resourceManager);
        cardCollection.GenerateTestDeck(20);

        deck = new Deck(this, cardCollection, eventManager);
        Debug.Log("deck initialized with cards : " + deck.GetCount());

        hand = new CardHand(this, eventManager);
    }

    protected virtual void Start() {
        tableManager.AssignFieldsToPlayer(this);
    }
}
