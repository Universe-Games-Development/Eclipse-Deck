using UnityEngine;
using Zenject;

public class GameBoardController : MonoBehaviour {
    [SerializeField] public CreatureSpawner CreatureSpawner;
    // Scene Context
    [Inject] private GameboardBuilder _boardManager;
    [Inject] private TurnManager _turnManager;

    [Header("Grid Interaction Params")]
    [Range(0, 10)]
    public float yInteractionRange = 1f;

    // Game context
    [Inject] private GridVisual boardVisual;

    public Field GetFieldByWorldPosition(Vector3? mouseWorldPosition) {
        if (!mouseWorldPosition.HasValue || boardVisual == null) {
            return null;
        }

        Transform origin = boardVisual.GetBoardOrigin();
        Vector3 worldPosition = mouseWorldPosition.Value;

        // Check if the click is within the y range
        if (Mathf.Abs(worldPosition.y - origin.position.y) > yInteractionRange) {
            return null;
        }

        // Get grid index by world position
        Vector2Int? gridIndex = _boardManager.GridBoard.GetGridIndexByWorld(origin, worldPosition);
        if (!gridIndex.HasValue) {
            return null;
        }

        // ѕолучаем поле по индексам
        Field field = _boardManager.GridBoard.GetFieldAt(gridIndex.Value.x, gridIndex.Value.y);

        // ≈сли поле не может быть выбрано Ц выводим предупреждение и выходим
        if (field == null || !IsValidFieldSelected(field)) {
            if (field != null) {
                Debug.LogWarning($"Can't select field: {field.GetTextCoordinates()}");
            }
            return null;
        }

        //Debug.Log($"Selected: {field.GetTextCoordinates()}");
        return field;
    }

    public bool IsValidFieldSelected(Field field) {
        if (!IsInitialized()) {
            Debug.LogWarning("Gameboard not initialized! Can't select field");
            return false;
        }

        if (!_boardManager.GridBoard.FieldExists(field)) {
            return false;
        }

        if (_turnManager.ActiveOpponent != field.Owner) {
            Debug.LogWarning("Field does not belong to the current player.");
            return false;
        }

        return true;
    }

    public bool IsInitialized() {
        if (_boardManager.GridBoard == null || _boardManager.GridBoard.Config == null) {
            Debug.LogWarning("GridManager is not properly initialized: Global grid is null or empty.");
            return false;
        }

        return true;
    }
}

