using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class TestCard3DWrapper : MonoBehaviour {
    [SerializeField] private Card3DView card3DView;

    [SerializeField] int updateTimes;
    private System.Random random; // ��������� ��������� ���������� ����� ��� ������� ���������� �����

    private void Awake() {
        random = new System.Random(Guid.NewGuid().GetHashCode()); // ������������ ���������� ���������� ��� ������� ����������
        card3DView.OnInitialized += () => {
            
            Test();
        };
    }

    private void Test() {
        //TestUpdateHealth().Forget();
    }

    private async UniTask TestUpdateHealth() {
        for (int i = 0; i < updateTimes; i++) {
            int randomHealth = random.Next(1, 100); // �������� ��������� �������� � ������� 1-100
            card3DView.CardInfo.UpdateHealth(randomHealth);
            int randomAttack = random.Next(1, 100); // �������� ��������� �������� � ������� 1-100
            card3DView.CardInfo.UpdateAttack(randomAttack);
            Debug.Log($"Update! Iteration {i}, Health: {randomHealth}");
            await UniTask.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
