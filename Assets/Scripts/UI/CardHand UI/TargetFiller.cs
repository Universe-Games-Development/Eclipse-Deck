using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class OperationTargetsFiller : MonoBehaviour {
    private SelectorService selectorService;
    private OperationTargetsHandler targetHandler;
    private CancellationTokenSource globalCancellationSource = new CancellationTokenSource();

    [SerializeField] private HumanTargetSelector tempSelector; // REMOVE SOON 

    private void CancelFilling() {
        globalCancellationSource?.Cancel();
    }

    private void Start() {
        selectorService = new();
    }


    public bool CanBeFilled(List<NamedTarget> targetData) {
        return true; // Placeholder for actual logic
    }

    public async UniTask<Dictionary<string, GameUnit>> FillTargetsAsync(List<NamedTarget> namedTargets, CancellationToken cancellationToken) {

        List<NamedTarget> workingTargets = new List<NamedTarget>(namedTargets);
        targetHandler = new OperationTargetsHandler(workingTargets);

        while (targetHandler.HasNextTarget()) {
            ITargetRequirement targetRequirement = targetHandler.GetCurrentRequirement();
            string name = targetHandler.GetCurrentTargetName();
            Debug.Log($"Awaiting targets for : {name}");

            GameUnit gameUnit = await tempSelector.SelectTargetAsync(targetRequirement, name);
            targetHandler.TrySetTarget(gameUnit);
        }

        return targetHandler.GetFilledTargets(); // Placeholder for actual logic
    }
}
public class OperationTargetsHandler {
    private List<NamedTarget> targets;
    private int currentTargetIndex;

    public OperationTargetsHandler(List<NamedTarget> targets) {
        this.targets = targets;
        FindNextEmptyTarget();
    }

    public bool HasNextTarget() {
        return currentTargetIndex < targets.Count;
    }

    public ITargetRequirement GetCurrentRequirement() {
        if (!HasNextTarget()) return null;
        return targets[currentTargetIndex].Requirement;
    }

    public string GetCurrentTargetName() {
        if (!HasNextTarget()) return null;
        return targets[currentTargetIndex].Name;
    }

    public bool TrySetTarget(GameUnit unit) {
        if (unit == null) return false;
        if (!HasNextTarget()) return false;

        // Проверяем, не записан ли уже этот объект
        var existingTargetIndex = FindTargetWithUnit(unit);
        if (existingTargetIndex != -1) {
            // Отменяем выбор существующего объекта
            targets[existingTargetIndex].Unit = null;

            // Если отмененный объект был до текущего, переходим к нему
            if (existingTargetIndex < currentTargetIndex) {
                currentTargetIndex = existingTargetIndex;
            }
            return false;
        }
        // Записываем объект
        targets[currentTargetIndex].Unit = unit;
        FindNextEmptyTarget();
        return true;
    }

    private int FindTargetWithUnit(GameUnit unit) {
        for (int i = 0; i < targets.Count; i++) {
            if (targets[i].Unit != null && targets[i].Unit == unit) {
                return i;
            }
        }
        return -1;
    }

    private void FindNextEmptyTarget() {
        for (int i = 0; i < targets.Count; i++) {
            if (targets[i].Unit == null) {
                currentTargetIndex = i;
                return;
            }
        }
        currentTargetIndex = targets.Count; // Все заполнены
    }

    public Dictionary<string, GameUnit> GetFilledTargets() {
        var result = new Dictionary<string, GameUnit>();
        foreach (var target in targets) {
            if (target.Unit != null) {
                result[target.Name] = target.Unit;
            }
        }
        return result;
    }
}

public interface ITargetSelector {
    public abstract UniTask<GameUnit> SelectTargetAsync(ITargetRequirement requirement, string targetName);
}


public class SelectorService {
    BoardGame boardGame; // will be used to search for game units matching the action requirements
    public bool IsPossibleAction(GameOperation action) {
        return true; // Placeholder for actual logic
    }
}

public enum OpponentType {
    Initiator,  // Той, хто ініціював операцію
    Opponent,   // Опонент ініціатора
    AnyPlayer,  // Будь-який гравець (для рідкісних випадків)
    SpecificPlayer // Для складних випадків (з можливістю вказати конкретного гравця)
}

// will be used to get model of game unit objects
public interface IGameUnitProvider {
    GameUnit GetUnit();
}