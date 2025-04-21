using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

// ������� ���� ��� ���������
public class Ability {
    private TriggerManager _triggerManager = new();
    public List<GameOperation> Operations = new();
    public IModifierProvider ModifierProvider { get; private set; }
    public Ability(List<AbilityTrigger> triggers, IModifierProvider modifierProvider) {
        _triggerManager.OnTriggerActivation += Activate;
        triggers.ForEach(trigger => {
            _triggerManager.AddTrigger(trigger);
        });
        ModifierProvider = modifierProvider;
    }

    // Sonn become command 
    private void Activate(Opponent opponent, IGameUnit unit, IEvent @event) {
        ActivateAsync(opponent).Forget();
    }

    // ���������� �������
    public async UniTask<bool> ActivateAsync(Opponent invoker) {
        bool atLeastOneOperationSucceeded = false;

        foreach (var operation in Operations) {
            await operation.FillRequirements(invoker);

            // ���� �� �������� ��� ��������, �������� ��������
            if (operation.AreTargetsFilled()) {
                List<IOperationModifier> operationModifiers = ModifierProvider.GetAll();
                foreach (var modifier in operationModifiers) {
                    modifier.Attach(operation);
                }

                operation.PerformOperation();

                foreach (var modifier in operationModifiers) {
                    modifier.Deatach();
                }
                atLeastOneOperationSucceeded = true;
            }
        }

        return atLeastOneOperationSucceeded;
    }
}

public abstract class GameOperation {
    [Inject] protected readonly IGameContext _data;

    // ������� ���� (�� PropertyInfo ��� ����� �������������) -> ������
    public Dictionary<string, IRequirement> Requirements { get; } = new Dictionary<string, IRequirement>();

    // ������� ��� ��������� ���������� ����������
    public Dictionary<string, object> Results { get; } = new Dictionary<string, object>();

    public abstract void PerformOperation();

    public async UniTask<bool> FillRequirements(Opponent requestingPlayer) {
        foreach (var entry in Requirements) {
            string fieldName = entry.Key;
            IRequirement requirement = entry.Value;

            ITargetingService filler = PickTargetingService(requirement.IsCasterFill ? requestingPlayer : _data.BoardSeatSystem.GetAgainstOpponent(requestingPlayer));
            object result = await filler.ProcessRequirementAsync(requestingPlayer, requirement);

            if (result == null) {
                return false;
            }
            SetTarget(fieldName, result);
        }
        return true;
    }

    public virtual void SetTarget(string key, object target) {
        Results[key] = target;
    }
    private ITargetingService PickTargetingService(Opponent inputPlayer) {
        return _data.BoardSeatSystem.GetActionFiller(inputPlayer);
    }

    // ���������, �� �� �������� ��� ��������
    public virtual bool AreTargetsFilled() {
        foreach (var req in Requirements) {
            if (!Results.ContainsKey(req.Key)) {
                return false;
            }
        }
        return true;
    }

    // ��������� ����� ��� ��������� ���������� � ���������� �����
    protected T GetResult<T>(string fieldName) where T : class {
        if (Results.TryGetValue(fieldName, out object value)) {
            return value as T;
        }
        return null;
    }
}

// �������� ��������� �����
public class DealDamageOperation : GameOperation {
    private string TARGET_KEY = "target";
    private IDamageDealer _dealer;
    public DealDamageOperation(IDamageDealer dealer) {
        _dealer = dealer;

        // ������ ������ ������ �� ���
        CreatureRequirement creatureRequirement = new CreatureRequirement(new AnyCreatureCondition());
        Requirements.Add(TARGET_KEY, creatureRequirement);
    }

    public override void PerformOperation() {
        if (!AreTargetsFilled()) {
            Debug.LogError("����������� ����� ��� ��������");
            return;
        }

        var target = Results["target"] as IDamageable;
        if (target != null) {
            _dealer.Attack.DealDamage(target);
        }
    }
}

