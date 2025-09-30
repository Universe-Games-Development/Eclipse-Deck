using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

[OperationFor(typeof(SummonOperationData))]
public class SummonCreatureOperation : GameOperation {
    private const string SpawnPlaceKey = "spawnZone";
    private readonly SummonOperationData _data;

    private readonly IEntityFactory entityFactory;
    private readonly ITargetFiller targetFiller;
    [Inject] IOperationFactory operationFactory;

    public SummonCreatureOperation(SummonOperationData data, IEntityFactory entityFactory, ITargetFiller targetFiller) {
        _data = data;
        this.entityFactory = entityFactory;
        this.targetFiller = targetFiller;
        
        AddTarget(new TargetInfo(SpawnPlaceKey, TargetRequirements.AllyPlace));
    }

    public override async UniTask<bool> Execute() {
        if (!TryGetTypedTarget(SpawnPlaceKey, out Zone zone)) {
            Debug.LogError($"Valid {SpawnPlaceKey} not found");
            return false;
        }

        if (Source is not CreatureCard creatureCard) {
            Debug.LogError($"{this}: Creature card is null");
            return false;
        }

        if (zone.IsFull()) {
            SacrificeCreatureOperation sacrificeCreatureOperation = operationFactory.Create<SacrificeCreatureOperation>();
            TargetInfo targetInfo = sacrificeCreatureOperation.GetTargets().First();
            // if received null return false
            TargetFillResult targetFillResult = await targetFiller.TryFillTargetAsync(targetInfo, Source, false);
            if (!targetFillResult.IsSuccess) {
                return false;
            }

            sacrificeCreatureOperation.SetTarget(targetInfo.Key, targetFillResult.Unit);
            bool success = await sacrificeCreatureOperation.Execute();
            if (!success) return false;
        }


        // 1. Створюємо істоту
        Creature _creature = entityFactory.CreateCreatureFromCard(creatureCard);
        if (_creature == null) return false;

        // 2. Створюємо візуальну задачу

        var summonTask = visualTaskFactory.Create<SummonFromCardVisualTask>(
            _creature, _data.visualTemplate
        );

        visualManager.Push(summonTask);

        await UniTask.DelayFrame(1);
        // 3. Виконуємо логіку
        return zone.TryPlaceCreature(_creature);
    }
}


// Transform Card into Creature
public class SummonFromCardVisualTask : VisualTask, IDisposable {
    private readonly Creature _creature;
    private readonly SummonVisualData _data;
    private readonly IUnitRegistry _unitRegistry;
    private readonly IUnitSpawner<Card, CardView, CardPresenter> _cardSpawner;
    private readonly ICardFactory _cardFactory;

    [Inject] IUnitSpawner<Creature, CreatureView, CreaturePresenter> _creatureSpawner;

    private CardPresenter _tempCardCopy;
    private Vector3 _spawnPosition;

    public SummonFromCardVisualTask(Creature creature, SummonVisualData data,
        IUnitRegistry unitRegistry, IUnitSpawner<Card, CardView, CardPresenter> cardSpawner,
        ICardFactory cardFactory) {

        _creature = creature;
        _data = data;
        _unitRegistry = unitRegistry;
        _cardSpawner = cardSpawner;
        _cardFactory = cardFactory;

        _tempCardCopy = CreateVisualCopy(creature);
    }

    public override async UniTask<bool> Execute() {
        try {
            // 1. Видаляємо тимчасову копію карти
            if (_tempCardCopy != null) {
                _cardSpawner.RemoveUnit(_tempCardCopy);
                _tempCardCopy = null;
            }

            // 2. Створюємо істоту на тій самій позиції
            var creaturePresenter = _creatureSpawner.SpawnUnit(_creature);
            var creatureView = creaturePresenter.CreatureView;
            creatureView.transform.position = _spawnPosition;

            // 3. Анімація материалізації
            await PlayMaterializationEffect(creatureView);

            return true;
        } finally {
            // Cleanup у будь-якому випадку
            SafeCleanupTempCard();
        }
    }

    private CardPresenter CreateVisualCopy(Creature creature) {
        var originalPresenter = _unitRegistry.GetPresenter<CardPresenter>(creature.SourceCard);
        if (originalPresenter?.View == null) {
            throw new InvalidOperationException($"Original card presenter not found for {creature.SourceCard}");
        }

        // Зберігаємо позицію одразу, поки оригінал ще існує
        _spawnPosition = originalPresenter.View.transform.position;

        // Створюємо копію карти
        var copyCard = _cardFactory.CreateCard(originalPresenter.Card.Data);
        var visualCardCopy = _cardSpawner.SpawnUnit(copyCard, registerInSystems: false); // Не реєструємо в системах

        // Налаштовуємо візуальну копію
        visualCardCopy.View.transform.position = _spawnPosition;
        visualCardCopy.View.transform.rotation = originalPresenter.View.transform.rotation;

        // Можливо, треба зменшити альфу або додати візуальний індикатор що це копія
        // visualCardCopy.View.SetAlpha(0.8f); 

        return visualCardCopy;
    }

    private async UniTask PlayMaterializationEffect(CreatureView creatureView) {
        // 1. Створюємо партикл ефект
        GameObject effect = null;
        if (_data.materializationEffect != null) {
            effect = GameObject.Instantiate(
                _data.materializationEffect,
                creatureView.transform.position,
                Quaternion.identity
            );
        }

        try {
            // 2. Анімація з'явлення істоти
            creatureView.transform.localScale = Vector3.zero;

            var scaleTween = creatureView.transform
                .DOScale(Vector3.one, _data.materializationDuration)
                .SetEase(Ease.OutBack);

            await creatureView.DoTweener(scaleTween);

            // 3. Додаткова затримка якщо потрібно
            if (_data.materializationDelay > 0) {
                await UniTask.Delay((int)(_data.materializationDelay * 1000));
            }
        } finally {
            // Очищуємо ефект
            if (effect != null) {
                GameObject.Destroy(effect, _data.effectLifetime);
            }
        }
    }

    private void SafeCleanupTempCard() {
        if (_tempCardCopy != null) {
            try {
                _cardSpawner.RemoveUnit(_tempCardCopy);
            } catch (Exception ex) {
                Debug.LogWarning($"Failed to cleanup temp card: {ex.Message}");
            } finally {
                _tempCardCopy = null;
            }
        }
    }

    public void Dispose() {
        SafeCleanupTempCard();
    }
}

public class SacrificeOperationData : OperationData {
}

public class SacrificeCreatureOperation : GameOperation {
    private const string TargetCreatureKey = "targetCreature";
    public SacrificeCreatureOperation(Zone zone = null) {
        TargetRequirement<Creature> targetRequirement;
        if (zone != null) {
            targetRequirement = new RequirementBuilder<Creature>()
                .WithCondition(new ZoneCondition(zone))
                .Build();
        } else {
            targetRequirement = new RequirementBuilder<Creature>().Build();
        }

        SimpleTargetInstruction simpleTargetInstruction = new SimpleTargetInstruction("Select creature to sacrifice");
        AddTarget(new TargetInfo(TargetCreatureKey, targetRequirement, simpleTargetInstruction));
    }

    public override async UniTask<bool> Execute() {
        if (!TryGetTypedTarget(TargetCreatureKey, out Creature creature)) {
            Debug.LogError($"Valid {TargetCreatureKey} not found");
            return false;
        }

        creature.Die();


        await UniTask.CompletedTask;
        return true;
    }
}
