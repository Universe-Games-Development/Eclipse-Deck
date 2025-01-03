using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BattleCreature : MonoBehaviour
{
    private Field currentField;
    private Animator animator;
    private AudioSource audioSource;

    [SerializeField] private AudioClip attackSound; // Звук атаки
    [SerializeField] private string attackAnimationTrigger = "Attack"; // Триггер анімації атаки
    [SerializeField] private Transform originalRotation; // Початкова орієнтація спрайта

    public Health health;
    private int Attack;
    public string Name;
    private AttackStrategy attackStrategy;

    private void Awake() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (!TryGetComponent(out health)) {
            gameObject.AddComponent(typeof(Health));
        }
    }

    public void Initialize(Card card, Field field, AttackStrategy newAttackStrategy) {
        Name = card.Name;
        Attack = card.Attack;
        health.SetHealth(card.Health);
        attackStrategy = newAttackStrategy;
        currentField = field;
        originalRotation = transform; // зберігаємо початкову орієнтацію спрайта
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
        animator.SetTrigger(attackAnimationTrigger);
        audioSource.PlayOneShot(attackSound);

        // Виконуємо атаку
        attackStrategy.Attack(currentField, enemyFields, Attack);

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

    public void TakeDamage(int damage) {
        health.ApplyDamage(damage);

        Debug.Log($"{Name} отримує {damage} шкоди. Здоров'я: {health.GetHealth()}.");
    }
}
