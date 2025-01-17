using UnityEngine;
using Zenject;

public class EnemyController : MonoBehaviour
{
    private Animator animator;
    [Inject] public Enemy enemy;
}
