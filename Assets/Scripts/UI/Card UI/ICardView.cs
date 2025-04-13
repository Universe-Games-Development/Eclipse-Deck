using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

public interface ICardView {
    void InitializeAnimator();
    void OnPointerClick(PointerEventData eventData);
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit(PointerEventData eventData);
    UniTask RemoveCardView();
    void Reset();
    void SetCardData(CardData cardData);
    void SetInteractable(bool value);
}