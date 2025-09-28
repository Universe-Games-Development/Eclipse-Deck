using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using Zenject;
using static UnityEngine.UI.Image;

[OperationFor(typeof(SummonOperationData))]
public class SummonCreatureOperation : GameOperation {
    private const string SpawnPlaceKey = "spawnZone";
    private readonly SummonOperationData _data;

    private readonly ICreatureFactory creatureFactory;

    public SummonCreatureOperation(SummonOperationData data, ICreatureFactory creatureFactory) {
        _data = data;
        this.creatureFactory = creatureFactory;

        PlaceRequirement allyZone = TargetRequirements.AllyPlace;
        AddTarget(SpawnPlaceKey, allyZone);
    }

    public override bool Execute() {
        if (!TryGetTypedTarget(SpawnPlaceKey, out Zone zone)) {
            Debug.LogError($"Valid {SpawnPlaceKey} not found");
            return false;
        }

        if (Source is not CreatureCard creatureCard) {
            Debug.LogError($"{this}: Creature card is null");
            return false;
        }

        // 1. ��������� ������
        Creature _creature = creatureFactory.CreateModel(creatureCard);
        if (_creature == null) return false;

        // 2. ��������� �������� ������

        var summonTask = visualTaskFactory.Create<SummonFromCardVisualTask>(
            _creature, _data.visualTemplate
        );

        visualManager.Push(summonTask);

        // 3. �������� �����
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
    private bool _isDisposed = false;
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
        if (_isDisposed) {
            Debug.LogError("Attempting to execute disposed SummonFromCardVisualTask");
            return false;
        }

        try {
            // 1. ��������� ��������� ���� �����
            if (_tempCardCopy != null) {
                _cardSpawner.RemoveUnit(_tempCardCopy);
                _tempCardCopy = null;
            }

            // 2. ��������� ������ �� �� ���� �������
            var creaturePresenter = _creatureSpawner.SpawnUnit(_creature);
            var creatureView = creaturePresenter.CreatureView;
            creatureView.transform.position = _spawnPosition;

            // 3. ������� �������������
            await PlayMaterializationEffect(creatureView);

            return true;
        } catch (Exception ex) {
            Debug.LogError($"Summon visualization failed: {ex.Message}");
            return false;
        } finally {
            // Cleanup � ����-����� �������
            SafeCleanupTempCard();
        }
    }

    private CardPresenter CreateVisualCopy(Creature creature) {
        var originalPresenter = _unitRegistry.GetPresenter<CardPresenter>(creature.SourceCard);
        if (originalPresenter?.View == null) {
            throw new InvalidOperationException($"Original card presenter not found for {creature.SourceCard}");
        }

        // �������� ������� ������, ���� ������� �� ����
        _spawnPosition = originalPresenter.View.transform.position;

        // ��������� ���� �����
        var copyCard = _cardFactory.CreateCard(originalPresenter.Card.Data);
        var visualCardCopy = _cardSpawner.SpawnUnit(copyCard, registerInSystems: false); // �� �������� � ��������

        // ����������� �������� ����
        visualCardCopy.View.transform.position = _spawnPosition;
        visualCardCopy.View.transform.rotation = originalPresenter.View.transform.rotation;

        // �������, ����� �������� ����� ��� ������ ��������� ��������� �� �� ����
        // visualCardCopy.View.SetAlpha(0.8f); 

        return visualCardCopy;
    }

    private async UniTask PlayMaterializationEffect(CreatureView creatureView) {
        // 1. ��������� ������� �����
        GameObject effect = null;
        if (_data.materializationEffect != null) {
            effect = GameObject.Instantiate(
                _data.materializationEffect,
                creatureView.transform.position,
                Quaternion.identity
            );
        }

        try {
            // 2. ������� �'������� ������
            creatureView.transform.localScale = Vector3.zero;

            var scaleTween = creatureView.transform
                .DOScale(Vector3.one, _data.materializationDuration)
                .SetEase(Ease.OutBack);

            await creatureView.DoTweener(scaleTween);

            // 3. ��������� �������� ���� �������
            if (_data.materializationDelay > 0) {
                await UniTask.Delay((int)(_data.materializationDelay * 1000));
            }
        } finally {
            // ������� �����
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
        if (_isDisposed) return;

        SafeCleanupTempCard();
        _isDisposed = true;

        GC.SuppressFinalize(this);
    }

    ~SummonFromCardVisualTask() {
        if (!_isDisposed) {
            Debug.LogWarning("SummonFromCardVisualTask was not properly disposed");
            Dispose();
        }
    }
}


