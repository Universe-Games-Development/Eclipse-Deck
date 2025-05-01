using System;
using UnityEngine;

public interface ICardTextureRenderer {

}

public interface I3dCardFactory {

}

public class CardHand3DView : MonoBehaviour, ICardHandView {
    private ICardTextureRenderer cardTextureRenderer;
    private I3dCardFactory cardFactory;

    event Action<string> ICardHandView.CardClicked {
        add {
            throw new NotImplementedException();
        }

        remove {
            throw new NotImplementedException();
        }
    }

    public void Cleanup() {
        throw new NotImplementedException();
    }

    public ICardView CreateCardView(string id) {
        throw new NotImplementedException();
    }

    public void DeselectCardView(string id) {
        throw new NotImplementedException();
    }

    public void RemoveCardView(string id) {
        throw new NotImplementedException();
    }

    public void SelectCardView(string id) {
        throw new NotImplementedException();
    }

    public void SetInteractable(bool value) {
        throw new NotImplementedException();
    }

    public void Toggle(bool value = true) {
        throw new NotImplementedException();
    }
}
