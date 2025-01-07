using UnityEngine;

public interface IObjectDistributer {
    void Initialize();
    GameObject CreateObject();
    void ReleaseObject(GameObject obj);
}