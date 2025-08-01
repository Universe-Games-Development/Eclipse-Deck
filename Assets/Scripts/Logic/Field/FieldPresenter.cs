using Cysharp.Threading.Tasks;

// Clean presenter pattern implementation
public class FieldPresenter {
    public readonly Field Model;
    public readonly FieldView View;

    public FieldPresenter(Field field, FieldView view) {
        this.Model = field;
        this.View = view;

        HandleOwnerChanged(field.Owner);
        HandleTypeChanged(field.FieldType);

        // Subscribe to field events
        field.OwnerChanged += HandleOwnerChanged;
        field.TypeChanged += HandleTypeChanged;
        field.FieldRemoved += HandleFieldRemoved;
    }


    public void Cleanup() {
        // Unsubscribe from events
        Model.OwnerChanged -= HandleOwnerChanged;
        Model.TypeChanged -= HandleTypeChanged;
        Model.FieldRemoved -= HandleFieldRemoved;
    }

    // Event handlers
    private void HandleOwnerChanged(BoardPlayer opponent) {
        View.UpdateOwnerVisuals(opponent);
    }

    private void HandleTypeChanged(FieldType type) {
        View.UpdateTypeVisuals(type);
    }

    private void HandleFieldRemoved(Field field) {
        View.RemoveWithAnimation().Forget();
    }
}
