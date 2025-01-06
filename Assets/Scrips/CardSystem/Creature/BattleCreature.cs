using System.Collections;
using UnityEngine;
using Zenject;


public class BattleCreature : MonoBehaviour {
    public Card card;

    private Field currentField;
    private Animator animator;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private AudioClip attackSound; // ���� �����
    [SerializeField] private string attackAnimationTrigger = "Attack"; // ������� ������� �����
    [SerializeField] private Transform originalRotation; // ��������� �������� �������
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
        originalRotation = transform; // �������� ��������� �������� �������
        spriteRenderer.sprite = card.MainImage;

        GameContext gameContext = new();
        gameContext.sourceCreature = this;

        eventManager = card.EventManager; ;
        await eventManager.TriggerEventAsync(EventType.ON_CREATURE_SUMMONED, gameContext);
        uIManager.CreateCreatureUI(this, card);

        card.ChangeState(CardState.OnTable);
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
        if (animator != null) {
            animator.SetTrigger(attackAnimationTrigger);
        } else {
            Debug.LogWarning("Animator is missing on BattleCreature!");
        }

        audioSource.PlayOneShot(attackSound);

        // �������� �����
        attackStrategy.Attack(currentField, enemyFields, card.Attack.CurrentValue);

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
