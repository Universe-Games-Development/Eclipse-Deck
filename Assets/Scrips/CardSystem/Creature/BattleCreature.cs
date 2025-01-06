using System.Collections;
using UnityEngine;
using Zenject;


public class BattleCreature : MonoBehaviour {
    public Card card;

    private Field currentField;
    private Animator animator;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private AudioClip attackSound; // Звук атаки
    [SerializeField] private string attackAnimationTrigger = "Attack"; // Триггер анімації атаки
    [SerializeField] private Transform originalRotation; // Початкова орієнтація спрайта
    public string Name;
    private AttackStrategy attackStrategy;

    [Inject] private IEventManager eventManager;
    [Inject] private UIManager uIManager;

    private void Awake() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private async void Death() {
        GameContext gameContext = new();
        gameContext.sourceCreature = this;
        await eventManager.TriggerEventAsync(EventType.ON_CARD_DISCARDED, gameContext);
        card.ChangeState(CardState.Discarded);
    }

    public async void Initialize(Card card, AttackStrategy newAttackStrategy, Field currentField) {
        this.card = card;
        Name = card.Name;
        attackStrategy = newAttackStrategy;
        originalRotation = transform; // зберігаємо початкову орієнтацію спрайта
        spriteRenderer.sprite = card.MainImage;

        GameContext gameContext = new();
        gameContext.sourceCreature = this;

        eventManager = card.EventManager; ;
        await eventManager.TriggerEventAsync(EventType.ON_CREATURE_SUMMONED, gameContext);
        uIManager.CreateCreatureUI(this, card);

        card.ChangeState(CardState.OnTable);
    }

    public void PerformAttack(Field[] enemyFields) {
        // Відтворення анімації атаки та звуку
        StartCoroutine(AttackCoroutine(enemyFields));
    }

    private IEnumerator AttackCoroutine(Field[] enemyFields) {
        // Знаходимо ціль
        Field targetField = GetTargetField(enemyFields);
        Vector3 targetPosition = targetField.transform.position;

        // Повертаємо спрайт до цілі
        yield return RotateToTarget(targetPosition);

        // Відтворюємо анімацію атаки
        if (animator != null) {
            animator.SetTrigger(attackAnimationTrigger);
        } else {
            Debug.LogWarning("Animator is missing on BattleCreature!");
        }

        audioSource.PlayOneShot(attackSound);

        // Виконуємо атаку
        attackStrategy.Attack(currentField, enemyFields, card.Attack.CurrentValue);

        // Повертаємось в початкову орієнтацію після завершення атаки
        yield return new WaitForSeconds(1f); // Дочекаємось завершення анімації атаки
        yield return RotateToTarget(originalRotation.position); // Повертаємось до початкової позиції
    }

    private Field GetTargetField(Field[] enemyFields) {
        // Логіка вибору цілі для атаки (можна вибирати за індексом або іншим методом)
        return enemyFields[0]; // просто для прикладу
    }

    private IEnumerator RotateToTarget(Vector3 targetPosition) {
        // Повертаємо спрайт в напрямку цілі
        Vector3 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Обчислюємо кут

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle)); // Задаємо орієнтацію

        // Плавно повертаємось до цілі
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f) {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            yield return null;
        }

        transform.rotation = targetRotation; // Переконуємось, що ми точно повернулись в кут
    }

    public bool Silence() {
        foreach (var ability in card.abilities) {
            ability.UnregisterActivation();
        }
        return true;
    }

    public object GetAttack() {
        return card.Attack;
    }
}
