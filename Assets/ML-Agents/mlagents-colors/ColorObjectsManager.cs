using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ColorObjectsManager : MonoBehaviour {
    [Header("Префаб і настройки")]
    [SerializeField] private ColorObject colorObjectPrefab;
    [SerializeField] private float spacing = 2.0f;
    [SerializeField] private int objectCount = 6;
    [SerializeField] private float spawnHeight = 3f; // Висота спавну для падіння

    [Header("Режим розташування")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.Mixed;

    [Header("Доступні кольори")]
    [SerializeField] private List<Color> availableColors = new List<Color>();
    [SerializeField] private List<string> availableColorNames = new List<string>();

    private List<ColorObject> colorObjects = new List<ColorObject>();
    [SerializeField] Transform spawnContainer;
    [SerializeField] float startAngular = 1f;
    [SerializeField] private float spawnJitter = 1f;

    public enum SpawnMode {
        Mixed,      // Змішаний (як зараз)
        Ordered     // Впорядкований згідно індексів
    }


    private string GetColorNameFromRGB(Color color) {
        float r = color.r;
        float g = color.g;
        float b = color.b;
        float a = color.a;

        var standardColors = new Dictionary<string, Color> {
            {"red", Color.red}, {"green", Color.green}, {"blue", Color.blue},
            {"yellow", Color.yellow}, {"cyan", Color.cyan}, {"magenta", Color.magenta},
            {"white", Color.white}, {"black", Color.black}, {"gray", Color.gray}
        };

        string closestColor = "custom";
        float minDistance = float.MaxValue;

        foreach (var standardColor in standardColors) {
            float distance = ColorDistance(color, standardColor.Value);
            if (distance < minDistance) {
                minDistance = distance;
                closestColor = standardColor.Key;
            }
        }

        if (minDistance < 0.1f) {
            return closestColor;
        }

        return GenerateDescriptiveColorName(color);
    }

    private float ColorDistance(Color c1, Color c2) {
        float rDiff = c1.r - c2.r;
        float gDiff = c1.g - c2.g;
        float bDiff = c1.b - c2.b;
        return Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
    }

    private string GenerateDescriptiveColorName(Color color) {
        List<string> components = new List<string>();
        float brightness = (color.r + color.g + color.b) / 3f;

        if (brightness < 0.2f) components.Add("dark");
        else if (brightness > 0.8f) components.Add("light");

        float maxComponent = Mathf.Max(color.r, color.g, color.b);
        float threshold = 0.3f;

        if (color.r >= maxComponent - threshold) components.Add("red");
        if (color.g >= maxComponent - threshold) components.Add("green");
        if (color.b >= maxComponent - threshold) components.Add("blue");

        if (components.Count >= 3 && Mathf.Abs(color.r - color.g) < 0.2f &&
            Mathf.Abs(color.g - color.b) < 0.2f) {
            if (brightness > 0.7f) return "light gray";
            if (brightness < 0.3f) return "dark gray";
            return "gray";
        }

        return string.Join(" ", components);
    }

    public void Initialize(int count) {
        if (spawnContainer == null) spawnContainer = transform;

        objectCount = Mathf.Min(count, availableColors.Count);

        if (objectCount > availableColors.Count) {
            Debug.LogWarning($"Requested {count} objects but only {availableColors.Count} colors available. Using {availableColors.Count} objects.");
            objectCount = availableColors.Count;
        }

        CreateColorObjects();
    }

    private void CreateColorObjects() {
        ClearColorObjects();

        for (int i = 0; i < objectCount; i++) {
            ColorObject colorObj = Instantiate(colorObjectPrefab, transform);

            Vector3 position = CalculateSpawnPosition(i);
            colorObj.transform.position = position;

            // Додаємо рандомний поштовх для більш реалістичного падіння
            Rigidbody rb = colorObj.GetComponent<Rigidbody>();
            if (rb != null) {
                Vector3 randomTorque = new Vector3(
                    Random.Range(-startAngular, startAngular),
                    Random.Range(-startAngular, startAngular),
                    Random.Range(-startAngular, startAngular)
                );
                rb.AddTorque(randomTorque, ForceMode.Impulse);
            }

            colorObjects.Add(colorObj);
        }

        AssignColorsToObjects();
    }

    private Vector3 CalculateSpawnPosition(int index) {
        Vector3 position = Vector3.zero;

        if (spawnMode == SpawnMode.Ordered) {
            // --- Лінійне розташування ---
            float totalWidth = (objectCount - 1) * spacing;
            Vector3 startPos = spawnContainer.position - new Vector3(totalWidth / 2f, 0f, 0f);
            position = startPos + new Vector3(index * spacing, spawnHeight, 0f);
        } else {
            // --- Колове розташування для змішаного режиму ---
            float angle = index * (360f / objectCount);
            Vector3 basePosition = spawnContainer.position +
                Quaternion.Euler(0, angle, 0) * Vector3.forward * spacing;

            // Додаємо випадкове зміщення тільки для змішаного режиму
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnJitter, spawnJitter),
                0f,
                Random.Range(-spawnJitter, spawnJitter)
            );

            position = basePosition + randomOffset;
            position.y = spawnHeight;
        }

        return position;
    }


    private void AssignColorsToObjects() {
        for (int i = 0; i < colorObjects.Count && i < availableColors.Count; i++) {
            colorObjects[i].SetupColorObject(availableColors[i], availableColorNames[i]);
        }
    }

    public void RandomizeColors() {
        if (availableColors.Count < colorObjects.Count) {
            Debug.LogError($"Not enough colors available! Have {availableColors.Count}, need {colorObjects.Count}");
            return;
        }


        // Для змішаного режиму - випадкове перемішування
        List<int> indices = new List<int>();
        for (int i = 0; i < colorObjects.Count; i++) {
            indices.Add(i);
        }

        for (int i = indices.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        for (int i = 0; i < colorObjects.Count; i++) {
            int colorIndex = indices[i];
            colorObjects[i].SetupColorObject(availableColors[colorIndex], availableColorNames[colorIndex]);
        }
    }

    public void AddColor(Color newColor) {
        availableColors.Add(newColor);
        availableColorNames.Add(GetColorNameFromRGB(newColor));

        if (colorObjects.Count < availableColors.Count) {
            UpdateColorObjectsCount();
        }
    }

    private void UpdateColorObjectsCount() {
        int newCount = availableColors.Count;

        while (colorObjects.Count < newCount) {
            ColorObject colorObj = Instantiate(colorObjectPrefab, transform);
            int index = colorObjects.Count;

            Vector3 position = CalculateSpawnPosition(index);
            colorObj.transform.position = position;
            colorObjects.Add(colorObj);
        }

        AssignColorsToObjects();
    }


    public ColorInfo CreateNewTargetColor() {
        if (colorObjects.Count == 0) {
            Debug.LogError("No color objects available!");
            return new ColorInfo(Color.white, "white");
        }

        int randomIndex = Random.Range(0, colorObjects.Count);
        ColorObject randomObject = colorObjects[randomIndex];

        return (randomObject.ColorInfo);
    }

    public List<ColorInfo> GetCurrentColors() {
        List<ColorInfo> currentColors = new();

        foreach (var obj in colorObjects) {
            currentColors.Add((obj.ColorInfo));
        }

        return currentColors;
    }

    public ColorObject GetColorObject(int index) {
        if (index >= 0 && index < colorObjects.Count) {
            return colorObjects[index];
        }
        return null;
    }

    public Vector3 GetColorObjectPosition(int index) {
        ColorObject obj = GetColorObject(index);
        return obj != null ? obj.transform.position : Vector3.zero;
    }

    public int GetObjectCount() {
        return colorObjects.Count;
    }

    public int GetAvailableColorsCount() {
        return availableColors.Count;
    }

    public List<ColorObject> GetAllColorObjects() {
        return new List<ColorObject>(colorObjects);
    }

    // Методи для управління режимом розташування
    public void SetSpawnMode(SpawnMode mode) {
        spawnMode = mode;
        CreateColorObjects(); // Перестворюємо об'єкти з новим режимом
    }

    public SpawnMode GetCurrentSpawnMode() {
        return spawnMode;
    }

    public void ToggleSpawnMode() {
        spawnMode = spawnMode == SpawnMode.Mixed ? SpawnMode.Ordered : SpawnMode.Mixed;
        CreateColorObjects(); // Перестворюємо об'єкти з новим режимом
    }

    private void ClearColorObjects() {
        foreach (var obj in colorObjects) {
            if (obj != null) {
                Destroy(obj.gameObject);
            }
        }
        colorObjects.Clear();
    }

    private void OnDestroy() {
        ClearColorObjects();
    }

    public int FindColorIndex(Color color) {
        for (int i = 0; i < colorObjects.Count; i++) {
            ColorObject colorObject = colorObjects[i];

            ColorInfo colorInfo = colorObject.ColorInfo;

            if (colorInfo != null) {
                if (colorInfo.color == color) {
                    return i;
                }
            }
        }

        return -1;
    }

    public ColorObject GetColorObjectByColor(Color color) {
        foreach (var item in colorObjects) {
            if (item.ColorInfo == null) continue;
            if (item.ColorInfo.color == color) return item;
        }
        return null;
    }
}