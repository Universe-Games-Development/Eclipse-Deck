using UnityEngine;
using Zenject.SpaceFighter;
using Zenject;
public class CreatureView : MonoBehaviour {
    [SerializeField]
    Animator _animator = null;

    [SerializeField]
    MeshRenderer _renderer = null;

    [SerializeField]
    MeshFilter _mesh = null;

    public Animator Animator { get => _animator; set => _animator = value; }
    public MeshRenderer Renderer { get => _renderer; set => _renderer = value; }
    public MeshFilter Mesh { get => _mesh; set => _mesh = value; }
}