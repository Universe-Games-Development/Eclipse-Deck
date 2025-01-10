using UnityEngine;

[System.Serializable]
public class BoardConfig {
    public Transform FieldsOrigin;
    public float VerticalOffset = 0f;
    public float HorizontalOffset = 0f;
    public int NumberOfColumns = 7;
    public Vector3 FieldSize = new Vector3(0f, 0f, 0f);
    public FieldType[] FieldTypeFormat;
}
