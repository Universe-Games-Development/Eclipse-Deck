using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BattleCreature : MonoBehaviour
{
    private Field currentField;
    private Animator animator;
    private AudioSource audioSource;

    [SerializeField] private AudioClip attackSound; // ���� �����
    [SerializeField] private string attackAnimationTrigger = "Attack"; // ������� ������� �����
    [SerializeField] private Transform originalRotation; // ��������� �������� �������

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
        originalRotation = transform; // �������� ��������� �������� �������
    }

    public void PerformAttack(Field[] enemyFields) {
        // ³��������� ������� ����� �� �����
        StartCoroutine(AttackCoroutine(enemyFields));
    }

    private IEnumerator AttackCoroutine(Field[] enemyFields) {
        // ��������� ����
        Field targetField = GetTargetField(enemyFields);
        Vector3 targetPosition = targetField.transform.position;

        // ��������� ������ �� ���
        yield return RotateToTarget(targetPosition);

        // ³��������� ������� �����
        animator.SetTrigger(attackAnimationTrigger);
        audioSource.PlayOneShot(attackSound);

        // �������� �����
        attackStrategy.Attack(currentField, enemyFields, Attack);

        // ����������� � ��������� �������� ���� ���������� �����
        yield return new WaitForSeconds(1f); // ���������� ���������� ������� �����
        yield return RotateToTarget(originalRotation.position); // ����������� �� ��������� �������
    }

    private Field GetTargetField(Field[] enemyFields) {
        // ����� ������ ��� ��� ����� (����� �������� �� �������� ��� ����� �������)
        return enemyFields[0]; // ������ ��� ��������
    }

    private IEnumerator RotateToTarget(Vector3 targetPosition) {
        // ��������� ������ � �������� ���
        Vector3 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // ���������� ���

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle)); // ������ ��������

        // ������ ����������� �� ���
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f) {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            yield return null;
        }

        transform.rotation = targetRotation; // ������������, �� �� ����� ����������� � ���
    }

    public void TakeDamage(int damage) {
        health.ApplyDamage(damage);

        Debug.Log($"{Name} ������ {damage} �����. ������'�: {health.GetHealth()}.");
    }
}
