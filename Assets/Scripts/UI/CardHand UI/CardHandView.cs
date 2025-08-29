using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandView : MonoBehaviour {
    [SerializeField] private HandLayoutStrategy layoutStrategy;
    public virtual void Toggle(bool value = true) {
        if (gameObject.activeSelf != value) {
            gameObject.SetActive(value);
        }
    }

    public CardTransform[] GetCardTransforms(int cardCount) {
        return layoutStrategy.CalculateCardTransforms(cardCount);
    }

    public abstract CardView BuildCardView();
}

public abstract class HandLayoutStrategy : MonoBehaviour {
    public abstract CardTransform[] CalculateCardTransforms(int cardCount);
}

[System.Serializable]
public struct CardTransform {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public int sortingOrder;

    public CardTransform(Vector3 pos, Quaternion rot, Vector3 scl, int sorting = 0) {
        position = pos;
        rotation = rot;
        scale = scl;
        sortingOrder = sorting;
    }
}


