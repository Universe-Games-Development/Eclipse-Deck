using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayCardUI : MonoBehaviour
{
    [SerializeField] private GameObject ui;

    private void Awake() {
        ui.SetActive(false);
    }

    public async UniTask<bool> FillInputs(Card card) {
        ui.SetActive(true);
        await UniTask.Delay(4000);
        ui.SetActive(false);
        return false;
    }
}
