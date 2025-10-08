using UnityEngine;

public class MusicTse : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    [SerializeField] AudioSource source;

    private void Start() {
        source.clip = clip;
    }
}
