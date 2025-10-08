using UnityEngine;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour
{
    [SerializeField] Button recalculateButton;
    [SerializeField] BoardPresenter boardPresenter;

    private void Awake() {
       // recalculateButton.onClick.AddListener(() => );
    }
}
