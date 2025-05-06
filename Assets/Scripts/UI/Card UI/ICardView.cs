using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class CardView : MonoBehaviour {
    public event Action<CardView> OnCardClicked;

    public CardUIInfo CardInfo;
    [SerializeField] protected bool isInteractable = true;
    private void Awake() {
        InitializeAnimator();
    }

    public string Id { get; set; }
    
    public abstract void InitializeAnimator();

    public virtual async UniTask RemoveCardView() {
        // Base implementation for card removal
        await UniTask.CompletedTask;
        Destroy(gameObject);
    }

    public virtual void Select() {
        // Base card selection behavior
    }

    public virtual void Deselect() {
        // Base card deselection behavior
    }

    public virtual void Reset() {
        // Reset card to default state
        isInteractable = false;
    }

    public virtual void SetInteractable(bool value) {
        isInteractable = value;
    }

    protected virtual void RaiseCardClickedEvent() {
        if (isInteractable) {
            OnCardClicked?.Invoke(this);
        }
    }
}