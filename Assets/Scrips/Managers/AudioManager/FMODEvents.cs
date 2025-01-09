using FMODUnity;
using UnityEngine;


public class FMODEvents: MonoBehaviour
{
    [field: Header("Ambient")]
    [field: SerializeField] public EventReference sewersAmbient { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference testMusic {  get; private set; }

}
