using UnityEngine;
using Zenject;

public class Opponent : MonoBehaviour
{
    public string Name = "Opponent";
    protected CardCollection cardCollection;
    protected Deck deck;
    protected Deck discardDeck;

    protected CardHand hand;
    protected Health health;

    private ResourceManager resourceManager;
    private IEventManager eventManager;

    [SerializeField] protected TableManager tableManager;

    [Inject]
    public void Construct(IEventManager eventManager, ResourceManager resourceManager) {
        this.eventManager = eventManager;
        this.resourceManager = resourceManager;
    }

    protected virtual void Awake() {

        cardCollection = new CardCollection(resourceManager);
        cardCollection.GenerateTestDeck(20);

        deck = new Deck(this, cardCollection, eventManager);
        Debug.Log("deck initialized with cards : " + deck.GetCount());

        hand = new CardHand();
    }

    public void TakeDamage(int damage) {
        health.ApplyDamage(damage);

        Debug.Log($"{gameObject} ������ {damage} �����. ������'�: {health.GetHealth()}.");
    }
}