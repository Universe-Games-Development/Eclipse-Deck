using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioClip m_Clip;
    private AudioSource m_Source;

    private void Start() {
        m_Source = GetComponent<AudioSource>();
        m_Source.clip = m_Clip;
        m_Source.Play();
    }
}
