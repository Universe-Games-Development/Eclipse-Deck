using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class ColorObject : MonoBehaviour {
    [Header("Components")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TextMeshPro labelText;

    [Header("Highlight Settings")]
    [SerializeField] private float maxEmissionIntensity = 2f; // Максимальний рівень яскравості

    [SerializeField] ColorInfo colorInfo;
    public ColorInfo ColorInfo => colorInfo;

    private MaterialPropertyBlock _block;
    private Sequence _emissionTween;

    // Кешуємо ID властивостей шейдера для продуктивності
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    void Awake() {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (labelText == null)
            labelText = GetComponentInChildren<TextMeshPro>();

        _block = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(_block);
    }

    public void SetupColorObject(Color color, string name) {
        colorInfo = new ColorInfo(color, name);

        gameObject.name = $"ColorObject_{name}";

        ChangeBodyColor(color);

        if (labelText != null)
            labelText.text = name;
    }

    public void ChangeBodyColor(Color color) {
        if (_block == null) return;
        meshRenderer.GetPropertyBlock(_block);
        _block.SetColor(BaseColorID, color);
        _block.SetColor(EmissionColorID, Color.black); // Вимикаємо світіння за замовчуванням
        meshRenderer.SetPropertyBlock(_block);
    }

    /// <summary>
    /// Анімує світіння об'єкта: спалах та плавне затухання.
    /// </summary>
    /// <param name="highlightColor">Колір світіння.</param>
    /// <param name="duration">Загальна тривалість анімації.</param>
    public void HighlightOverTime(Color highlightColor, float duration) {
        // Зупиняємо попередню анімацію, якщо вона ще активна, щоб уникнути конфліктів
        if (_emissionTween != null && _emissionTween.IsActive()) {
            _emissionTween.Kill();
        }

        // Отримуємо поточні властивості матеріалу, щоб не перезатерти інші налаштування
        meshRenderer.GetPropertyBlock(_block);

        // Встановлюємо колір для світіння (Emission)
        _block.SetColor(EmissionColorID, highlightColor);

        // Створюємо послідовність анімацій (Sequence) для спалаху
        _emissionTween = DOTween.Sequence();

        // Фаза 1: Наростання інтенсивності світіння (половина загального часу)
        // Ми анімуємо значення від 0 до maxEmissionIntensity.
        // На кожному кроці (onUpdate) ми застосовуємо це значення до матеріалу.
        _emissionTween.Join(
            DOTween.To(
                getter: () => 0f,
                setter: value => {
                    _block.SetFloat("_EmissionIntensity", value); // Unity's URP/HDRP uses this property name
                    meshRenderer.SetPropertyBlock(_block);
                },
                endValue: maxEmissionIntensity,
                duration: duration * 0.5f)
            .SetEase(Ease.OutQuad) // Робимо початок анімації різким, а кінець плавним
        );

        // Фаза 2: Затухання інтенсивності світіння (друга половина часу)
        _emissionTween.Append(
            DOTween.To(
                getter: () => maxEmissionIntensity,
                setter: value => {
                    _block.SetFloat("_EmissionIntensity", value);
                    meshRenderer.SetPropertyBlock(_block);
                },
                endValue: 0f,
                duration: duration * 0.5f)
            .SetEase(Ease.InQuad) // Робимо початок плавним, а кінець різким
        );

        // Гарантуємо, що після завершення анімації світіння повністю вимкнене
        _emissionTween.OnComplete(() => {
            _block.SetFloat("_EmissionIntensity", 0f);
            meshRenderer.SetPropertyBlock(_block);
        });

        _emissionTween.Play();
    }

    public Color GetColor() {
        return ColorInfo != null ? ColorInfo.color : Color.black;
    }

    public override string ToString() {
        return ColorInfo != null ? ColorInfo.colorName : base.ToString();
    }

    private void OnDestroy() {
        transform.DOKill();
    }
}

[Serializable]
public class ColorInfo {
    public Color color;
    public string colorName;

    public ColorInfo(Color color, string name) {
        this.color = color;
        colorName = name;
    }
}
