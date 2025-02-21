using UnityEngine;
using Zenject;

public class GameBoardController : MonoBehaviour {
    // Scene Context
    [Inject] private GameBoard gameBoard;
    [Inject] private GameBoardManager boardManager;
    [SerializeField] public CreatureSpawner CreatureSpawner;

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
        Vector2Int? gridIndex = boardManager.GridBoard.GetGridIndexByWorld(origin, worldPosition);
        if (!gridIndex.HasValue) {
            return null;
        }

        // ѕолучаем поле по индексам
        Field field = boardManager.GridBoard.GetFieldAt(gridIndex.Value.x, gridIndex.Value.y);

        // ≈сли поле не может быть выбрано Ц выводим предупреждение и выходим
        if (field == null || !gameBoard.IsValidFieldSelected(field)) {
            if (field != null) {
                Debug.LogWarning($"Can't select field: {field.GetTextCoordinates()}");
            }
            return null;
        }

        Debug.Log($"Selected: {field.GetTextCoordinates()}");
        return field;
    }
}

