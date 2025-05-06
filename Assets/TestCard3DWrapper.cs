using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class TestCard3DWrapper : MonoBehaviour {
    [SerializeField] RenderCell renderCell;
    [SerializeField] private Card3DView card3DView;
    [SerializeField] int updateTimes;
    private void Start() {
        CardUIView uiView = renderCell.Register3DCard(card3DView);
        card3DView.SetUiReference(uiView);
        TestUpdateHealth().Forget();
    }

    private async UniTask TestUpdateHealth() {
        for (int i = 0; i < updateTimes; i++) {
            card3DView.CardInfo.UpdateHealth(i + 1);
            Debug.Log("Update! " + i);
            await UniTask.Delay(TimeSpan.FromSeconds(2));
        }
    }

}

